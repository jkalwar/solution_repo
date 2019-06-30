using MLAPI.Domain;
using MLAPI.Domain.Models;
using MLAPI.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity.Migrations;
using System.Web;

namespace MLAPI.Controllers
{
    [RoutePrefix("api/Experiment")]
    public class ExperimentsController : ApiController
    {
        /**
         * This method will generate the experiments for the given model and saves the data for all the experiments in the database and 
         * stores the best accuracy for this model over these experiments in the database
         * */
        [Route("GenerateExperiment")]
        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage GenerateExperiments(MLApiExperiment experiment)
        {
            ModelService modelservice = new ModelService();
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (experiment.ModelId == null || experiment.ModelId == Guid.Empty || !modelservice.IsValidModelId(experiment.ModelId ?? Guid.Empty))
            {
                dict.Add("status", "failure");
                dict.Add("error", "Valid model id is required");
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.NoContent, dict);
                return response;
            }
            else
            {
                double[] learningRates = new double[] { 0.001, 0.01, 0.1 };
                double[] stepsArr = new double[] { 1000, 2000, 4000 };
                double[] noOfLayers = new double[] { 1, 2, 4 };
                double maxlr = 0, maxsteps = 0, maxlayers = 0, maxacc = 0;

                List<String> experimentErrors = new List<String>();
                using (var mlapictx = new MLAPIEntities())
                {
                    for (int i = 0; i < learningRates.Length; i++)
                    {
                        for (int j = 0; j < stepsArr.Length; j++)
                        {
                            for (int k = 0; k < noOfLayers.Length; k++)
                            {
                                double acc = TrainModelAndReturnAccuracy(k, j, i);
                                if (acc > maxacc)
                                {
                                    maxlayers = noOfLayers[k];
                                    maxlr = learningRates[i];
                                    maxsteps = stepsArr[j];
                                    maxacc = acc;
                                }

                                try
                                {
                                    var models = mlapictx.Experiments;
                                    var newModel = new Experiment { ModelId = experiment.ModelId, Accuracy = (decimal)acc, LearningRate = (decimal?)learningRates[i], Steps = (decimal?)stepsArr[j], NumberOfLayers = (decimal?)noOfLayers[k] };
                                    mlapictx.Experiments.Add(newModel);
                                    mlapictx.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    var res = string.Format($"Error occurred for learning rate = ${i}  , steps = ${j} , layers: ${k} , message =  {ex.Message}");
                                    experimentErrors.Add(res);
                                    //The errors while training for a specfic paramters combination should be logged in splunk
                                }
                            }
                        }
                    }

                }
                //Sending the error response only if all the 27 experiments are  unsuccessful , even if some of them are successful sending
                //The response will be given with the best accuracy , steps , learning rate , no.of layers
                if (experimentErrors.Count != 27)
                {
                    dict.Add("status", "success");
                    dict.Add("Accuracy", maxacc);
                    dict.Add("Steps", maxsteps);
                    dict.Add("LearningRate", maxlr);
                    dict.Add("Layers", maxlayers);
                    var accparam = new MLApiAccuracyParameters { ModelId = experiment.ModelId, Accuracy = (decimal?)maxacc, Steps = (decimal?)maxsteps, LearningRate = (decimal?)maxlr, NumberOfLayers = (decimal?)maxlayers };
                    SaveBestResults(accparam);
                    return Request.CreateResponse(HttpStatusCode.OK, dict);
                }
                dict.Add("status", "failure");
                dict.Add("error", experimentErrors);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, dict);
            }


        }

        //This method return the training accuracy for each of the parameters for the model by running the python script : train.py with these paramters
        private double TrainModelAndReturnAccuracy(double layers, double steps, double learningRate)
        {
            var pythones = new PythonExecutionService().GetAccuracy(layers , steps , learningRate);
            //Call the python function and get the accuracy
            return pythones;
        }

        private void SaveBestResults(MLApiAccuracyParameters parameters)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            using (var mlapictx = new MLAPIEntities())
            {
                try
                {
                    var accparams = mlapictx.AccuracyParamters;
                    var data = mlapictx.AccuracyParamters.FirstOrDefault(x => x.ModelId == parameters.ModelId);
                    if (data != null)
                    {
                        data.Accuracy = parameters.Accuracy;
                        data.NumberOfLayers = parameters.NumberOfLayers;
                        data.Steps = parameters.Steps;
                        data.LearningRate = parameters.LearningRate;
                        mlapictx.Entry(data).State = System.Data.Entity.EntityState.Modified;
                        mlapictx.SaveChanges();
                    }
                    else
                    {
                        var newModel = new MLAPI.Domain.AccuracyParamter { ModelId = parameters.ModelId, Steps = parameters.Steps, NumberOfLayers = parameters.NumberOfLayers, Accuracy = parameters.Accuracy, LearningRate = parameters.LearningRate };
                        mlapictx.AccuracyParamters.Add(newModel);
                        mlapictx.SaveChanges();
                    }

                }
                catch (Exception ex)
                {
                    //Log the exception
                    var res = string.Format($"Error occurred  while saving the best results : {ex.Message}");
                }
            }
        }

        /*
         * This model return the best accuracy and the respective parameters for a model id.
         */

        [Route("GetBestAccuracy")]
        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage GetBestAccuracy()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            ModelService modelService = new ModelService();
            var httpRequest = HttpContext.Current.Request;
            //Getting the GUID from the request
            var guid = httpRequest.Params["ModelId"];
            var accparams = new MLApiAccuracyParameters();
            try
            {
                if (!modelService.IsValidModelId(Guid.Parse(guid)))
                {
                    var message = string.Format("The model is not available in the datamodel");
                    dict.Add("status", "failure");
                    dict.Add("error", message);
                    return Request.CreateResponse(HttpStatusCode.NotFound, dict);
                }
                accparams.ModelId = Guid.Parse(guid);
            }
            catch (Exception ex)
            {
                var message = string.Format("The GUID format is incorrect");
                dict.Add("status", "failure");
                dict.Add("error", message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
            }


            using (var mlapictx = new MLAPIEntities())
            {
                try
                {
                    var accparamsmodel = mlapictx.AccuracyParamters;
                    var data = mlapictx.AccuracyParamters.FirstOrDefault(x => x.ModelId == accparams.ModelId);
                    if(data == null)
                    {
                        dict.Add("status", "failure");
                        dict.Add("error", $"Accuracy paramters for the given modelid {accparams.ModelId} are not available");
                        return Request.CreateResponse(HttpStatusCode.OK, dict);
                    }
                    else
                    {
                        dict.Add("status", "success");
                        dict.Add("result",  new { ModelId = data.ModelId , Accuracy = data.Accuracy , Steps = data.Steps , LearningRate = data.LearningRate , Layers = data.NumberOfLayers });
                        return Request.CreateResponse(HttpStatusCode.OK, dict);
                    }
                }
                catch (Exception ex)
                {
                    dict.Clear();
                    dict.Add("status", "failure");
                    dict.Add("error", $"Error occurred while getting the best accuracy results : {ex.Message}");
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, dict);

                }

        }
        }
    }
}
   
