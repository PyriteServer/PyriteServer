// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="ISecretsProvider.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Contracts
{
    public interface ISecretsProvider
    {
        string Value { get; }
    }
}