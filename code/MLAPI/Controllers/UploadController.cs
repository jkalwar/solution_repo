using MLAPI.Domain;
using MLAPI.Domain.Models;
using MLAPI.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace MLAPI.Controllers
{
    [RoutePrefix("api/Upload")]
    public class UploadController : ApiController
    {
        /*
         * Method to receive the posted image
         */
        [Route("PostUserImage")]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> PostUserImage()
        {
            
            Dictionary<string, object> dict = new Dictionary<string, object>();
            try
            {
                ModelService modelService = new ModelService();
                var httpRequest = HttpContext.Current.Request;
                //Getting the GUID from the request
                var guid = httpRequest.Params["guid"];
                var image = new MLApiImage();
                try
                {
                    if (!modelService.IsValidModelId(Guid.Parse(guid)))
                    {
                        var message = string.Format("The model is not available in the datamodel");
                        dict.Add("status" , "failure");
                        dict.Add("error", message);
                        return Request.CreateResponse(HttpStatusCode.NotFound, dict);
                    }
                    image.ModelId = Guid.Parse(guid);
                }
                catch (Exception ex)
                {
                    var message = string.Format("The GUID format is incorrect");
                    dict.Add("status", "failure");
                    dict.Add("error", message);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                }

                foreach (string file in httpRequest.Files)
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);

                    var postedFile = httpRequest.Files[file];
                    if (postedFile != null && postedFile.ContentLength > 0)
                    {

                        int MaxContentLength = 1024 * 1024 * 1; //Size = 1 MB  

                        IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".png" };
                        var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                        var extension = ext.ToLower();

                        //Checking for the compatible extensions
                        if (!AllowedFileExtensions.Contains(extension))
                        {
                            var message = string.Format("Please Upload image of type .jpg,.png.");

                            dict.Add("status", "failure");
                            dict.Add("error", message);
                            return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                        }

                        else if (postedFile.ContentLength > MaxContentLength)
                        {
                            var message = string.Format("Please Upload a file of size upto 1 mb.");
                            dict.Add("status", "failure");
                            dict.Add("error", message);
                            return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                        }
                        else
                        {
                            string pathToCreate = "~/Userimages/";
                            if (!Directory.Exists(HttpContext.Current.Server.MapPath(pathToCreate)))
                            {
                                //Now you know it is ok, create it
                                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(pathToCreate));
                            }
                           
                            var filePath = HttpContext.Current.Server.MapPath("~/Userimages/" + postedFile.FileName + extension);
                            postedFile.SaveAs(filePath);
                            image.ImagePath = filePath;
                        }
                    }

                    var result = SaveImage(image);

                    return result;
                }
                var res = string.Format("No image found , please upload an image.");
                dict.Add("status", "failure");
                dict.Add("error", res);
                return Request.CreateResponse(HttpStatusCode.NotFound, dict);
            }
            catch (Exception ex)
            {
                var res = string.Format($"Error Occurred in saving image {ex.Message}");
                dict.Add("status" ,"failure");
                dict.Add("error", res);
                return Request.CreateResponse(HttpStatusCode.NotFound, dict);
            }
        }

        /*
         * Method to add the image path to the persistanct storage.
         * 
         */
        public HttpResponseMessage  SaveImage(MLApiImage image)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (string.IsNullOrEmpty(image.ImagePath))
            {
                dict.Add("status" , "failure");
                dict.Add("error", "Image path is not added");
            }
            else
            {
                using (var mlapictx = new MLAPIEntities())
                {

                    try
                    {
                        var images = mlapictx.Images;
                        var newModel = new Image { ModelId = image.ModelId,ImagePath = image.ImagePath };
                        mlapictx.Images.Add(newModel);

                        mlapictx.SaveChanges();

                        var responseMessage = string.Format("Image uploaded successfully.");
                        dict.Add("status", "success");
                        dict.Add("message" , responseMessage);
                        return Request.CreateResponse(HttpStatusCode.OK, dict); ;

                    }
                    catch (Exception ex)
                    {
                        var res = string.Format($"Error Ocuurred while uploading the image : {ex.Message}");
                        dict.Clear();
                        dict.Add("status", "failure");
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

