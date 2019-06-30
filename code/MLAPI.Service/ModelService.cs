using MLAPI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLAPI.Service
{
    public class ModelService
    {   
        /*
         *Utility method to check if the Model Guid  is valid 
         * 
         */
        public bool IsValidModelId(Guid guid)
        {

            using (var mlapictx = new MLAPIEntities())
            {

                try
                {
                    var models = mlapictx.Models;
                    var newModel = new Model { Id = guid};

                    
                    if(mlapictx.Models.Any(x => x.Id == guid))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    //Log the exception
                    var res = string.Format($"Error ocuurred while checking model validity");
                    return false;
                }
            }
        }

    }
}
