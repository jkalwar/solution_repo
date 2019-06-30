###############################################################################
# The following were done for the API design

# The api is hpsted in the azure app service PaaS service.
# API Base URL : ml-training-api.azurwebsites.net
# The data model for the api is in the SQL PaaS data base on Azure on this server :
# modeltraining.database.windows.net Databse Name : modeltrainingapi
# The API is developed in C# using ASP.NET WEB API framework
# Python scripts are integrated to the API backend services to run the training with the incoming parameters.

# Assumptions
# 1. This is a machine learning training API , If I understand it correctly the images that the user uploads will need to be
# stored somewhere so that the ML model can be run on those images.Ideally the images are supposed to be stored somewhere on the storage account in cloud and metadata should be in the datamodel so that python scripts can get the images and the model can be trained on those images.
# For simplicity , the uploaded images will be stored on the server in a folder and the folder path will be stored in the datamodel.
# I had to integrate the python scripts with the C# code in order to train and test. there was a syntax error in the last line of both the scripts and in the interest of time I went ahead and corrected it to use it. I have added those scripts also in the scripts folder.

# There were many edge cases in the process which require good logging. Whereever I felt the need , I have added //Log comments in the code.


# API Endpoints :
# 1. POST : api/Model/CreateModel/ Parameters : ModelName : Name of the Model , returns the guid of the model. The model meta data is stored in the Models table
# 2. POST : api/Upload/PostUserImage Parameters : Image file with either .jpg , .png extension and with size <= 1 MB. This endpoint can only upload one image at one time. The image path on the server/blob is updated in the Images table.
# 3. POST : api/Experiment/GenerateExperiment Parameters : ModelId (GUID) of the model - This endpoint runs the model training using the train.py file for 27 experiments with different combinations of the layers , learning rate , steps. For each experiment the accuracy output is stored in the Experiments table. This api also selects the combination of layers , steps and learning rate for the best accuracy and updates the AccuracyParamters table with the parameters that gave the best accuracy for a particular ModelId.
# 4. GET : api/Experiment/GetBestAccuracy Parameters : ModelId - Gets the paramters for the best accuracy for a specific ModelId.
# 5. POST: api/Test/TestUserImage Parameters : Image file .jpg OR .png extension with size < =1 MB. This api will give the accuracy of testing the model with a specific ModelId , by using the paramters for the best accuracy

# Test Scripts : ml_api_test.py
# Use python 3.6 and above

####################################################################################################################################
# 1. Create a Model with a model name and getting the GUID of the model:
# importing the requests library 
import requests 
import os
import sys

# defining the api-endpoint 
API_ENDPOINT = "http://ml-training-api.azurewebsites.net/api/Model/CreateModel"

print('This is a test script for the machine learning api - please give the input to create and train the model')
print()
# data to be sent to api 
model_name = input("Enter the name of the model you want to create (Name of length >3 && < 50 characters): ")
if len(model_name) > 50 or len(model_name) < 3:
    print("Length of string beyond 50 charcters or less than 3 characters , Try again")
    model_name = input("Enter the name of the model you want to create (Name of length >3 && < 50 characters): ")
else:
   print("Length of string beyond 50 charcters or less than 3 characters , Try again by rerunning the script")
data = {'ModelName':model_name}

# sending post request and saving response as response object 
response = requests.post(url = API_ENDPOINT, data = data) 


# extracting response text 
guidData = response.json()
print()
print("The guid of the model with name " + model_name + " is:%s "%guidData['guid']); 

print()
print()

###################################################################################################################################

# 2. Upload images to the api
API_ENDPOINT = "http://ml-training-api.azurewebsites.net/api/Upload/PostUserImage"

print('Let us upload the images , so that the above model can be trained on this data')

directory = input("Please give the folder that contains the images with .jpg OR .png format (Please give a path that doesnot contain spaces): ")
print(os.path.exists(directory))

if os.path.exists(directory) == False:
    print("The given directory does not exist please give a correct directory")   
    directory = input("Please give the folder that contains the images with .jpg OR .png format (Please give a path that doesnot contain spaces): ")
else:
    if(os.path.exists(directory) == False):
       print("The given directory does not exist please give a correct directory by rerunnin the script") 
       quit()

for file in os.listdir(directory):
     filename = os.fsdecode(file)
     if filename.endswith(".jpg") or filename.endswith(".png"): 
         print(os.path.join(directory, filename))
         files = {'media': open(os.path.join(directory, filename), 'rb')}
         params =  {'guid' : guidData['guid']}
         response = requests.post(API_ENDPOINT, files=files , params=params)

         responseJson = response.json()

         if responseJson['status'] == 'success':
            print(responseJson['message'])
         else:
            print(responseJson['error'])
         continue
     else:
         continue

print()
print("All the valid images in the folder are uploaded into a specific location on the server , from where the images can be pushed to a cloud storage as required.")
print("The image path is updated in the database with the model guid - to keep track of what images are used for training which model")

print()

print("Since we have the images , let us start the training")

#####################################################################################################################################
#3 . Generate the experiments by calling the train.py script with 27 combinations of the layers , steps , learning rate.
# Stores the accuracy data for each experiment int the database.
# Returns the best accuracy within the 27 experiments.

API_ENDPOINT = "http://ml-training-api.azurewebsites.net/api/Experiment/GenerateExperiment" 

print("Starting the training process for the model that is created above with GUID : " + guidData['guid'])

data = {'ModelId':guidData['guid']}

# sending post request and saving response as response object 
response = requests.post(url = API_ENDPOINT, data = data) 


# extracting response text 
bestData = response.json()
print()
print("The guid of the model with name " + model_name + " is:%s "%guidData['guid']); 

if bestData['status'] == 'success':
    print("The best accuracy obtained int this training session is : " + str(bestData['Accuracy']))
    print("Respective Steps : " + str(bestData['Steps']))
    print("Respective LearningRate : " + str(bestData['LearningRate']))
    print("Respective Layers : " + str(bestData['Layers']))
else:
    print("There are errors while running the training")
    print(bestData['error'])
    quit()
    
#####################################################################################################################################



####################################################################################################################################

#4 . Testing the model with a specfic test image , this returns the accuracy by running the model with the paramters that gave us the best accuracy for this  model.

print()

print("Now that we have trained the model with the images , let us test it")
print()
API_ENDPOINT = "http://ml-training-api.azurewebsites.net/api/Test/TestUserImage"

print('Please give the path of the image for which the model is to be tested.')
print()
filepath = input("Please give the path of the image in .jpg OR .png format (Please give a path that doesnot contain spaces): ")
#print(os.path.exists(filepath))
#print(filepath)

if os.path.exists(filepath) == False:
    print("The given directory does not exist please give a correct directory")   
    filepath = input("Please give the path of the image in .jpg OR .png format : ")
else:
    if(os.path.exists(filepath) == False):
       print("The given directory does not exist please give a correct directory by rerunning the script") 
       quit()

if filepath.endswith(".jpg") or filepath.endswith(".png"): 
    
    files = {'media': open(filepath, 'rb')}
    params =  {'guid' : guidData['guid']}
    response = requests.post(API_ENDPOINT, files=files , params=params)

    responseJson = response.json()
    #print(responseJson)
    if responseJson['status'] == 'success':
          print("The accuracy for the given test image is +" + str(responseJson['message']['Accuracy']))
    else:
        print(responseJson['error'])
        quit()

##################################################################################################################################
#5 - Get the best accuracy parameters for a specific modelid.
print("If you have a guid of one your models and you wish to see the combination of paramters that give the best accuracy Please give the model guid below")

API_ENDPOINT = "http://ml-training-api.azurewebsites.net/api/Experiment/GetBestAccuracy" 

guidValue = input("Please give the guid of your model : ")

params = {'ModelId':guidValue}

# sending post request and saving response as response object 
response = requests.get(url = API_ENDPOINT, params=params) 


# extracting response text 
bestData = response.json()
print()

if bestData['status'] == 'success':
    finalresult = bestData['result']
    print("The best accuracy recorded for the model id " + guidValue + " : " + str(finalresult['Accuracy']))
    print("Respective Steps : " + str(finalresult['Steps']))
    print("Respective LearningRate : " + str(finalresult['LearningRate']))
    print("Respective Layers : " + str(finalresult['Layers']))
else:
    print("There are errors while running the training")
    print(bestData['error'])
###########################################################################################################################
print()
print("Thank You , this is the end of the test file.")