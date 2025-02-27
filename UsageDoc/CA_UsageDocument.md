# Clearing Automation Tool

## Contents
<!--ts-->

* [Introduction](#introduction)

* [Clearing Automation tool workflow diagram](#clearing-automation-tool-workflow-diagram)


* [Prerequisite](#prerequisite)

* [Package Installation](#package-installation) 

* [Demo Project](#demo-project-after-consuming-the-package)

* [Clearing Automation Tool Execution](#clearing-automation-tool-execution)

  * [Package Identifier](#package-identifier) 

  * [SW360 Package Creator](#sw360-package-creator)

  * [Artifactory Uploader](#artifactory-uploader)
  
 * [Clearing Automation Tool Execution Test Mode](#clearing-automation-tool-execution-test-mode)

 * [How to handle multiple project types in same project](#how-to-handle-multiple-project-types-in-same-project)

* [Troubleshoot](#troubleshoot)
* [Manual Update](#manual-update)
* [Feedback](#feedback)
* [Bug or Enhancements](#bug-or-enhancements)
* [Glossary of Terms](#glossary-of-terms)
* [References](#references)

  * [Image References](#image-references)

  * [API References](#api-references)


<!--te-->
# Introduction

The Clearing Automation Tool (CA-Tool) helps the Project Manager/Developer to automate the sw360 clearing process of 3rd party components. This tool scans and identifies the third-party components used in a NPM, NUGET and Debian projects and makes an entry in SW360, if it is not present. CA-Tool links the components to the respective project and creates job for code scan in FOSSology.

CA-Tool reduces the effort in creating components in SW360 and identifying the matching source codes from the public repository. Tool eliminates the manual error while creating component and identifying correct version of source code from public repository. CA-Tool harmonize the creation of 3P components in SW360 by filling necessary information.
# Clearing Automation tool workflow diagram

- Package Identifier
   - [NPM/NUGET](../doc/usagedocimg/packageIdentifiernpmnuget.PNG)
   - [Debian](../doc/usagedocimg/packageIdentifierdebian.PNG)
- SW360 Package Creator
  - [NPM/NUGET](../doc/usagedocimg/packageCreatirnpmnuget.PNG)
  - [Debian](../doc/usagedocimg/packagecreatordebian.PNG)
- Artifactory Uploader
  - [NPM/NUGET](../doc/usagedocimg/artifactoryuploader.PNG)
# Prerequisite

1. Install Docker (Latest stable version).
2. **Make an entry for your project in SW360** for license clearance and is **should be in Active state** while running CA tool


4. **Access request :**

   Get SW360 REST API Authentication token

   > **_1.SW360 token :_**    
   
   > >a) The user can generate a token from their functional account. 

   > >b) The necessary credentials for token generation i.e the client id and client secret, could be obtained from the Sw360 team by creating an issue in their [repository](https://github.com/eclipse/sw360/issues)

   >**_2.Artifactory Token :_**

   >>a)For enabling the upload of cleared packages into jfrog artifactory, user's need to have their own jfrog artifactory credentials.This includes a username and an Apikey.

   **Pipeline Configuration :**

   For certain  scenarios we have predefined exit codes as mentioned below:

   |Exit Code| Scenario |
   |--|--|
   | 0 | Success |
   | 1 | Critical failure/error in the run |
   | 2 | Action item required from user's side |

   While configuring the CA Tool in the pipeline , user can configure each stage to display the result based on these exit codes. 
   
   This can be done by the configuration management team at the time of modifying the pipeline to support CA tool.
  
   After the configuration your pipeline will look like this : 
   
   ![folderpic](../doc/usagedocimg/piplinepic.PNG)

# Package Installation
   *  Open CMD/PowerShell to execute below command for getting latest CA Docker image,
         
        `docker pull <CA_DockerImageName>`

   * Use `docker images` command to verify the image is loaded properly.

  **Note**: You can explore and install particular CA Tool version [here](https://hub.docker.com/)  


# Demo project after consuming the package 
 You can find sample yml files under the [DemoProject](../DemoProject).

# Clearing Automation Tool Execution

1. **The Clearing Automation Tool has 3 dll &#39;s.**

    Execute them in the following order to achieve the complete License clearing process.
    
   > **1. Package Identifier**
      - Processes the input file and generates CycloneDX BOM file. The input file can be package file or a cycloneDx BOM file generated using the standard tool. If there are multiple input files, it can be processed by just passing the path to the directory in the argument.


   >**2. SW360 Package Creator**
      - Process the CycloneDX BOM file(i.e., output of the first dll) and creates the missing components/releases in SW360 and links all the components to the project in the SW360 portal. This exe also triggers the upload of the components to Fossology and automatically updates the clearing state in SW360.


   >**3. Artifactory Uploader**
      - Processes the CycloneDXBOM file(i.e., the output of the SW360PackageCreator) and uploads the already cleared components(clearing state-Report approved) to the siparty release repo in Jfrog Artifactory.The components in the states other than "Report approved" will be handled by the clearing experts via the Clearing Automation Dashboard.

2. **Prerequisite for CA Tool execution**
   
   - Create local directories for mapping to the CA tool container directories
        - Input  : Place to keep input files.
        - Output : Resulted files will be stored here.
        - Log    : CA log files.
        - CAConfig :  Place to keep Config files i.e., `appSettings.json`.

    **Note** : It is not recommended to use `Primary drive(Ex C:\)` for project execution or directory creation and also the `drive` should be configured as `Shared Drives` in docker.
   - Input files according to project type

      - **Project Type :** **NPM** 

          * Input file repository should contain **package-lock.json** file. If not present do an `npm install`.
          ![folderpic](../doc/usagedocimg/npminstall.PNG)
      
      - **Project Type :** **Nuget**
      
          * .Net core/.Net standard type project's input file repository should contain **package.lock.json** file. If not present do a `dotnet restore --use-lock-file`.
          
          * .Net Framework projects, input file repository should contain a **packages.config** file.
          
      - **Project Type :**  **Debian** 
      
   		       `Note : below steps is required only if you have tar file to process , otherwise you can keep `CycloneDx.json` file in the above created InputDirectory.`
          *  Create `InputImage` directory for keeping tar images and output directory will be above created `InputDirectory` .

          *  Run the command given below by replacing the place holder values (i.e., path to input image directory, path to input directory and file name of the Debian image to be cleared) with actual values.

           `docker run --rm -v <path/to/InputImageDirectory>:/tmp/InputImages -v <path/to/InputDirectory>:/tmp/OutputFiles clearingautomationtool ./syft packages /tmp/InputImages/<fileNameofthedebianImageTobeCleared.tar> -o cyclonedx-json --file "/tmp/OutputFiles/output.json"`
           
           After successful execution, `output.json` (_CycloneDX.json_) file will be created in specified output directory
           
           ![image.png](../doc/usagedocimg/output.PNG)
           
           Resulted `output.json` file will be having the list of installed packages  and the same file will be used as  an input to `CA- Bom creator` as an argument(`--packagefilepath`). The remaining process is same as other project types.

3. **Configuring the CA Tool**

   Copy the below content and create new `appSettings.json` file in `CAConfig` directory.

  
   Below is the list of settings can be made in `appSettings.json` file.

   _`Sample appSettings.json file`_

 
```
{
  "CaVersion": "3.0.0",
  "TimeOut": 200,
  "ProjectType": "<Insert ProjectType>",
  "SW360ProjectName": "<Insert SW360 Project Name>",
  "SW360ProjectID": "<Insert SW360 Project Id>",
  "Sw360AuthTokenType": "Bearer",
  "Sw360Token": "<Insert SW360Token in a secure way>",
  "SW360URL": "<Insert SW360URL>",
  "Fossologyurl": "<Insert Fossologyurl>",
  "JFrogApi": "<Insert JFrogApi>",
  "JfrogNugetDestRepoName": "JfrogNugetDestRepo Name",
  "JfrogNpmDestRepoName": "JfrogNpmDestRepo Name",
  "PackageFilePath": "/mnt/Input",
  "BomFolderPath": "/mnt/Output",
  "BomFilePath":"/mnt/Output/<SW360 Project Name>_Bom.cdx.json",
//IdentifierBomFilePath : For multiple project type 
  "IdentifierBomFilePath": "",
  "ArtifactoryUploadApiKey": "<Insert ArtifactoryUploadApiKey in a secure way>",
  "ArtifactoryUploadUser": "<Insert ArtifactoryUploadUser>",
  "RemoveDevDependency": true,
  "EnableFossTrigger": true,
  "InternalRepoList": [
    "<Repo1>",
    "<Repo2>"
  ],
  "Npm": {
    "Include": [ "p*-lock.json" ],
    "Exclude": [ "node_modules" ],
    "JfrogNpmRepoList": [
      "<Repo1>",
      "<Repo2>"
    ],
    "ExcludedComponents": []
  },
  "Nuget": {
    "Include": [ "pack*.config", "p*.lock.json" ],
    "Exclude": [],
    "JfrogNugetRepoList": [
      "<Repo1>",
      "<Repo2>"
    ],
    "ExcludedComponents": []
  },
  "Debian": {
    "Include": [ "*.json" ],
    "Exclude": [],
    "ExcludedComponents": []
  }
}
```

Description for the settings in `appSettings.json` file

|S.No| Argument name   |Description  | Is it Mandatory    | Example |
|--|--|--|--|--|
| 1 |--packagefilepath   | Path to the package-lock.json file or to the directory where the project is present in case we have multiple package-lock.json files.                                      |Yes  | D:\Clearing Automation|
| 2 |--cycloneDxbomfilePath | Path to the cycloneDx BOM file. This should not be used along with the package file path(arg no 1).Please note to give only  one type of input at a time.                           |No if the first argument is provided| D:\ExternalToolOutput|
| 3 |--bomfolderpath | Path to keep the generated boms  |  Yes     | D:\Clearing Automation\BOM
|  4| --sw360token  |  SW360 Auth Token |  Yes| Refer the SW360 Doc [here](https://www.eclipse.org/sw360/docs/development/restapi/access).Make sure you pass this credential in a secured way. |
| 5 | --sw360projectid |  Project ID from SW360 project URL of the project  |  Yes| Obtained from SW360 |
|  6|  --projecttype    | Type of the package         | Yes |  NPM/NUGET/Debian|
|7 | --removedevdependency  |  Make this field to `true` , if Dev dependencies needs to be excluded from clearing |  Optional ( By default set to true) | true/false |
| 8|  --sw360url  |  SW360 URL              |Yes |  https://<my_sw360_server>|
|  9| --sw360authtokentype   |  SW360 Auth Token  |Yes  | Token/Bearer |
|10  |  --settingsfilepath |  appSettings.json file path                                                                                                                             |Optional (By default it will take from the  bom creator exe location     |  |
|  11|  --artifactoryuploadapikey  | JFrog Auth Token          |  Yes| Generated from Jfrog Artifactory.Make sure you pass this credential in a secured way. |
|  12|  --bomfilepath  | CycloneDX BOM Filepath (output generated from the previous Package Identifier run) i.e The file path of the *_Bom.cdx.json file         |  Yes| For SW360PackageCreator & ArtifactoryUploader run needs to provide this path. |
|  13|  --identifierbomfilepath  | CycloneDX BOM Filepath (output generated from the previous Package Identifier run,applicable only if there are multiple project types) i.e The file path of the *_Bom.cdx.json file         |  No| If there are multiple project type this argument can be used. |
|  14|  --logfolderpath | Path to create log        |  No| If user wants to give configurable log path this parameter is used |
| 15   | --fossologyurl | Fossology URL                                                                                                                                        | Yes |      https://<my_fossology_server>                                                                                                                     | Yes                      |                                                                                                                          | Optional (By default it will take from the  Package Creator exe location     |                                                    |
| 16    | --artifactoryuploaduser              | Jfrog User Email                              | Yes                                                       |
| 17  | --jfrognpmdestreponame         | The destination folder name for the NPM package to be copied to                  | Yes                                                    |
| 18    | --jfrognugetdestreponame         | The destination folder name for the Nuget package to be copied to                  | Yes                                                    |
| 19    | --timeout          | SW360 response timeout value                  | No                                                       |                                                |

  #### Exclude  Component or Folders :
  In order to exclude any components ,it can be configured in the  "appSettings.json" by providing the package name and version as specified above in the *_ExcludedComponents_* field.

  Incase the component you want to exclude is of the format _"@group/componentname"_ `eg : @angular/common` specify it as _"@group/componentname:version"_ i.e `@angular/common:4.2.6`

  In order to **Exclude specific folders** from the execution, It can be specified under the **Exclude section** of that specific **package type**.


# CA Tool Execution

### Package Identifier

  - In order to run the PackageIdentifier.dll , execute the below command.

    **Example** : `docker run --rm -it -v /path/to/OutputDirectory:/mnt/Output -v /path/to/LogDirectory:/var/log -v /path/to/configDirectory:/etc/CATool clearingautomationtool dotnet PackageIdentifier.dll --settingsfilepath /etc/CATool/appSettings.json`




### SW360 Package Creator

  - In order to run the SW360PackageCreator.dll , execute the below command. 

    **Example** : `docker run --rm -it -v /path/to/OutputDirectory:/mnt/Output -v /path/to/LogDirectory:/var/log -v /path/to/configDirectory:/etc/CATool clearingautomationtool dotnet SW360PackageCreator.dll --settingsfilepath /etc/CATool/appSettings.json`


###  Artifactory Uploader

  * Artifactory uploader is **_`not applicable for Debian type package`_** clearance.

  *  In order to run the Artifactory Uploader dll , execute the below command.
  
     **Example** : `docker run --rm -it -v /path/to/OutputDirectory:/mnt/Output -v /path/to/LogDirectory:/var/log -v /path/to/configDirectory:/etc/CATool clearingautomationtool dotnet ArtifactoryUploader.dll --settingsfilepath /etc/CATool/appSettings.json`




# Clearing Automation Tool Execution Test Mode

  The purpose the test mode execution of the tool is to ensure that there are no any connectivity issues with SW360 server.
  
  - In order to execute the tool in test mode we need to pass an extra parameter to the existing 
argument list.
    
    **Example** : `docker run --rm -it -v /D/Projects/Output:/mnt/Output -v /D/Projects/DockerLog:/var/log -v /D/Projects/CAConfig:/etc/CATool clearingautomationtool dotnet ArtifactoryUploader.dll --settingsfilepath /etc/CATool/appSettings.json --mode test`

# How to handle multiple project types in same project

Incase your project has both NPM/Nuget components it can be handled by merely running then `Package Identifier dll` twice.
### Steps for Execution:
1. Run the `Package Identifier dll` with "**ProjectType**" set as "**NPM**" in `appSettings.json` .

2. A cycloneDX  BOM will be generated in the output BOM path that you provide in the argument.
3. Next run the `Package Identifier dll` with "**ProjectType**" set as "**NUGET**". In this run make sure that along with the usual arguments you also provide and additional argument "**--identifierBomFilePath**" which will contain the comparison BOM file path which is generated in the previous run.

4. Once this is done after the dll run you can find that the components from the first run for "**NPM**" and the components from second run for "**NUGET**" will be merged into one BOM file



# Troubleshoot
1. In case your pipeline takes a lot of time to run(more than 1 hour) when there are many components. It is advisable to increase the pipeline timeout and set it to a minimum of 1 hr.

1. In case of any failures in the pipeline, while running the tool,check the following configurations.
   * Make sure your build agents are running.
   * Check if there are any action items to be handled from the user's end.(In this case the exit code with which the pipeline will fail is **2**)

   * Check if the proxy settings environment variables for sw360 is rightly configured in the build machine.


# Manual Update
Upload attachment manually for [Debian](/UsageDoc/Manual-attachment-Debian-Overview.md) type.

# Feedback
Please add your feedbacks below

[Give feedback](https://github.com/siemens/continuous-clearing/issues)
# Bug or Enhancements

For reporting any bug or enhancement please follow below link

[Issues](https://github.com/siemens/continuous-clearing/issues)


# Glossary of Terms

| **3P Components** | **3rd Party Components**  |
|-------------------|---------------------------|
| BOM               | Bill of Material          |
| apiAuthToken      | SW360 authorization token |

# References
 ## Image References
- Fetching Project Id from SW360

![sw360pic](../doc/usagedocimg/sw360.PNG)


## API References 

- SW360 API Guide : [https://www.eclipse.org/sw360/docs/development/restapi/dev-rest-api/](https://www.eclipse.org/sw360/docs/development/restapi/dev-rest-api/)
- FOSSology API Guide: [https://www.fossology.org/get-started/basic-rest-api-calls/](https://www.fossology.org/get-started/basic-rest-api-calls/)

Copyright © Siemens AG ▪ 2023
