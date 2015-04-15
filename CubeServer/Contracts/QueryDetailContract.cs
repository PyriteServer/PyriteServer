// // //------------------------------------------------------------------------------------------------- 
// // // <copyright file="QueryDetailContract.cs" company="Microsoft Corporation">
// // // Copyright (c) Microsoft Corporation. All rights reserved.
// // // </copyright>
// // //-------------------------------------------------------------------------------------------------

namespace CubeServer.Contracts
{
    using System.Collections.Generic;

    public class QueryDetailContract
    {
        public IEnumerable<int[]> Cubes { get; set; }
        public string Name { get; set; }
    }
}