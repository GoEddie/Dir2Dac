using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dir2Dac;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;

namespace dir2dac.unittests
{
    [TestFixture]
    public class DdlScriptParserTests
    {
        [Test]
        public void Finds_Create_Proc_Statement()
        {
            var script = @"if object_id('do') is not null
          begin 
            drop procedure do
        end ;
		go
create procedure do as select 2;
    create table blah(i int)";

            var parser = new DdlScriptParser(SqlServerVersion.Sql100);
            var statements = parser.GetStatements(script);

            Assert.AreEqual(1,statements.Count );
            Assert.AreEqual(@"CREATE PROCEDURE do
AS
SELECT 2;
CREATE TABLE blah (
    i INT
);", statements.FirstOrDefault());      
        }

        [Test]
        public void Turns_Alter_Proc_Into_Create_Proc_Statement()
        {
            var script = @"
alter procedure do as select 2;
    create table blah(i int)";

            var parser = new DdlScriptParser(SqlServerVersion.Sql120);
            var statements = parser.GetStatements(script);

            Assert.AreEqual(1, statements.Count);
            Assert.AreEqual(@"CREATE PROCEDURE do
AS
SELECT 2;
CREATE TABLE blah (
    i INT
);", statements.FirstOrDefault());
        }
    }
}