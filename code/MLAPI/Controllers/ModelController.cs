using MLAPI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MLAPI.Domain.Models;

//Model Controller to manage the model creation
namespace MLAPI.Controllers
{
    [RoutePrefix("api/Model")]
    public class ModelController : ApiController
    {
        [Route("GetData")]
        [AllowAnonymous]
        [HttpGet]
        public IEnumerable<MLApiImage> AcessDB()
        {
            using (var x = new MLAPIEntities())
            {
                var res= x.Images.Select(z => new MLApiImage
                {
                    Id = z.Id,
                    ImagePath = z.ImagePath,
                    ModelId = z.ModelId
                }).ToList();
                return res;
            }
        }

        [Route("CreateModel")]
        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage CreateModel(MLApiModel model)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (string.IsNullOrEmpty(model.ModelName))
            {
                dict.Add("error", "Model name has to be given");
               
            }
            else
            {
                using (var mlapictx = new MLAPIEntities())
                {

                    try
                    {
                        var models = mlapictx.Models;
                        var newModel = new Model { ModelName = model.ModelName };
                        Guid obj = Guid.NewGuid();
                        newModel.Id = obj;
                        mlapictx.Models.Add(newModel);

                        mlapictx.SaveChanges();

                        var responseMessage = string.Format("Model created successfully.");
                        dict.Add("guid", newModel.Id);
                        return Request.CreateResponse(HttpStatusCode.OK, dict); ;

                    }
                    catch(Exception ex)
                    {
                        var res = string.Format($"Error Ocuurred while creating the model : {ex.Message}");
                        dict.Clear();
                        dict.Add("error", res);
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, dict);
                    }
                }
               
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.NoContent, dict);
            return response;
        }
    }
}
