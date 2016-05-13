using ManifestImportExportAPI.Models;
using System;

//*********************************************************************************
//**
//** IT IS LIKELY THAT THIS PART OF THE WEBSERVICE WILL NOT BE PUT INTO ACTION 
//** THE CURRENT THINKING IS THAT THE RETREIVE LATEST MANIFEST LIST WILL BE
//** DONE ON THE CLIENT SIDE ON WWW2.  THIS REMAINS HERE FOR POSSIBLE FUTURE USE 
//**
//********************************************************************************* 

namespace ManifestImportExportAPI.Repositories
{
    public interface IManifestRepository
    {
        RetrieveResults<ManifestList> RetrieveManifestList(DateTime date);
    }
}