# PLDI
PopLockDropIt is a configurable ransomware system created for research purposes. 
 
This uses an ASP.NET MVC5 IIS Web Application for the server-side and a C# console application for the client side. There are purposely no extensive features provided such as server hiding, socket connections, phishing UI, etc. This project has barebones necessary to perform a ransom exchange. This tool works exclusively on Windows and can used to test and configure IDS and monitoring systems to detect primitive ransomware actions. 
 
## Usage: Configure the desired settings in App.cs of the client

#### TARGET_FILE_EXTENSIONS =  { "hack", "png" }; 
target file examples would be to change to png, docx, doc, xlsx, ppt, pptx, etc; 
 
#### TARGET_DRIVE = $"{Environment.GetEnvironmentVariable("USERPROFILE")}\\Desktop\\";      
absolute path of interactions such as ransom note and decrypted files 
 
#### TARGET_FOLDER = $"{TARGET_DRIVE}ransom";  
absolute path to the top directory to search recursively and encrypt 
 
#### SERVER_ENDPOINT = "http://localhost:58271/"; 
the protocol, domain, and port to the server 
 
#### PRECRYPT_FILE_PATH = $"{TARGET_DRIVE}unencrypted.tar.gz"; 
absoltue path to the temporary archive/decrypted state 
 
#### POSTCRYPT_FILE_PATH = $"{TARGET_DRIVE}RANSOM.pwned"; 
absolute path to the remaining file signature of the AES encrypted file 
  
#### IS_DEBUG = false;  
enables file deletion after encryption 
