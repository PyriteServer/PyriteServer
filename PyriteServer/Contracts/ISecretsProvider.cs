// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="ISecretsProvider.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace PyriteServer.Contracts
{
    public interface ISecretsProvider
    {
        string Value { get; }
        bool Exists { get; }
    }
}