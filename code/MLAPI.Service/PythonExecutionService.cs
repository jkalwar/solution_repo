using MLAPI.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLAPI.Service
{
    public class PythonExecutionService
    {
        public double GetAccuracy(double layers, double steps, double learningRate)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            //start.FileName = @"C:\Users\Jayakrishna Alwar\AppData\Local\Programs\Python\Python36\python.exe";
            start.FileName = @"D:\Python34\python.exe";
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var scriptPath = basePath + @"Scripts\train.py";

            //The below is the path of the folder in which the images are stored before training
            //That path is given to the train.py file , the train.py file will read the images from that location and does the processing/training.
            //Ideally these images can be stored in a S3/Storage account and can be given to the model on demand for training by getting them into a folder 
            //and that folder with images will be used for training the model.S
            var im = basePath + @"UserImages\";
            var argv = $"\"--i\" \"{learningRate}\" \"--j\" \"{layers}\" \"--k\" \"{steps}\" \"--images\" \"{im}\"".Split(' ');
            start.Arguments = $"\"{scriptPath}\" \"{argv[0]}\" \"{argv[1]}\" \"{argv[2]}\" \"{argv[3]}\" \"{argv[4]}\" \"{argv[5]}\" \"{argv[6]}\" \"{argv[7]}\"";
            //args is path to .py file and any cm line args
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.CreateNoWindow = true;
            start.RedirectStandardError = true;

            var errors = "";
            var results = "";

            using (Process process = Process.Start(start))
            {
                errors = process.StandardError.ReadToEnd();
                results = process.StandardOutput.ReadToEnd();
            }
            var final = JsonConvert.DeserializeObject<dynamic>(results);
            var accuracy = final.accuracy.Value;
            return Convert.ToDouble(accuracy);
        }

        public double GetAccuracyForModel(Guid guid)
        {
            double steps = 0, learningrate = 0, layers = 0;
            using (var mlapictx = new MLAPIEntities())
            {
                try
                {
                    var accparams = mlapictx.AccuracyParamters;
                    var data = mlapictx.AccuracyParamters.FirstOrDefault(x => x.ModelId == guid);
                    if (data != null)
                    {
                        layers = (double)data.NumberOfLayers;
                        steps = (double)data.Steps;
                        learningrate = (double)data.LearningRate;
                    }
                    else
                    {
                        //Log that the guid is not present in the accuracy table
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    //Log the exception
                    var res = string.Format($"Error occurred  while getting the best results : {ex.Message}");
                    return -1;
                }

            }
             ProcessStartInfo start = new ProcessStartInfo();
            //start.FileName = @"C:\Users\Jayakrishna Alwar\AppData\Local\Programs\Python\Python36\python.exe";
            start.FileName = @"D:\Python34\python.exe";
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var scriptPath = basePath + @"Scripts\test.py";

            //The below is the path of the folder in which the images are stored before training
            //That path is given to the train.py file , the train.py file will read the images from that location and does the processing/training.
            //Ideally these images can be stored in a S3/Storage account and can be given to the model on demand for training by getting them into a folder 
            //and that folder with images will be used for training the model.S
            var im = basePath + @"UserImages\";
            var argv = $"\"--i\" \"{learningrate}\" \"--j\" \"{layers}\" \"--k\" \"{steps}\" \"--images\" \"{im}\"".Split(' ');
            start.Arguments = $"\"{scriptPath}\" \"{argv[0]}\" \"{argv[1]}\" \"{argv[2]}\" \"{argv[3]}\" \"{argv[4]}\" \"{argv[5]}\" \"{argv[6]}\" \"{argv[7]}\"";
            //args is path to .py file and any cm line args
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.CreateNoWindow = true;
            start.RedirectStandardError = true;

            var errors = "";
            var results = "";

            using (Process process = Process.Start(start))
            {
                errors = process.StandardError.ReadToEnd();
                results = process.StandardOutput.ReadToEnd();
            }
            var final = JsonConvert.DeserializeObject<dynamic>(results);
            var accuracy = final.accuracy.Value;
            return Convert.ToDouble(accuracy);
        }
    }
}
