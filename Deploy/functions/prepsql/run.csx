#load "..\CiqsHelpers\All.csx"

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.IO;
using System.Data.SqlClient;

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{

    var parametersReader = await CiqsInputParametersReader.FromHttpRequestMessage(req);

    string sqlConnectionString = parametersReader.GetParameter<string>("sqlConnectionString"); 

    string script = File.ReadAllText(@"d:\home\site\wwwroot\prepsql\prepareDatabase.sql");

    SqlConnection conn = new SqlConnection(sqlConnectionString);

    Server server = new Server(new ServerConnection(conn));

    server.ConnectionContext.ExecuteNonQuery(script);

    return new object();
}