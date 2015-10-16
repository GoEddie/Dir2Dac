using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dir2Dac;
using Microsoft.SqlServer.Dac.Model;
using NUnit.Framework;

namespace dir2dac.unittests
{
    [TestFixture]
    public class Arg_Parser_Tests
    {
        [Test]
        public void Can_Parse_Sql_Option_Bool()
        {

            var parser = new Args(new[] {"/do=trustworthy=true"});
            parser.Parse();
            Assert.IsTrue(parser.SqlModelOptions.Trustworthy != null && (bool) parser.SqlModelOptions.Trustworthy);
        }

        [Test]
        public void Can_Parse_Sql_Option_Int()
        {

            var parser = new Args(new[] { "/daTaBaseOptioN=ChangeTrackingRetentionPeriOD=123" });
            parser.Parse();
            Assert.IsTrue(parser.SqlModelOptions.ChangeTrackingRetentionPeriod!= null);
            Assert.AreEqual(123, parser.SqlModelOptions.ChangeTrackingRetentionPeriod);
        }


        [Test]
        public void Can_Parse_Sql_Option_String()
        {

            var parser = new Args(new[] { "/daTaBaseOptioN=Collation=EdwardRulz" });
            parser.Parse();
            Assert.IsTrue(parser.SqlModelOptions.Collation != null);
            Assert.AreEqual("EdwardRulz", parser.SqlModelOptions.Collation);
        }

        [Test]
        public void Can_Parse_Source_Path()
        {
            var parser = new Args(new[] { "/sourcePath=c:\\blah\\blah=*tSQLt*.sql" });
            parser.Parse();

            Assert.IsNotNull(parser.SourcePath.FirstOrDefault());
            Assert.AreEqual("c:\\blah\\blah", parser.SourcePath[0].Path);
            Assert.AreEqual("*tSQLt*.sql", parser.SourcePath[0].Filter);
        }

        [Test]
        public void Source_Filter_Defaults_To_StarDotSql()
        {
            var parser = new Args(new[] { "/sourcePath=c:\\blah\\blah" });
            parser.Parse();

            Assert.IsNotNull(parser.SourcePath[0]);
            Assert.AreEqual("*.sql", parser.SourcePath[0].Filter);
        }

        [Test]
        public void Can_Parse_PreCompare_Path()
        {
            var parser = new Args(new[] { "/PrECompare=c:\\blah\\blah\\ssss.sql" });
            parser.Parse();

            Assert.AreEqual("c:\\blah\\blah\\ssss.sql", parser.PreCompareScript);
        }

        [Test]
        public void Can_Parse_PostCompare_Path()
        {
            var parser = new Args(new[] { "/Postcompare=c:\\blah\\blah\\ssss.sql" });
            parser.Parse();

            Assert.AreEqual("c:\\blah\\blah\\ssss.sql", parser.PostCompareScript);
        }

        [Test]
        public void Can_Parse_Output_Path()
        {
            var parser = new Args(new[] { "/DP=c:\\blah\\blah\\767676\\" });
            parser.Parse();

            Assert.AreEqual("c:\\blah\\blah\\767676\\", parser.DacpacPath);
        }

        [Test]
        public void Can_Parse_Sql_Version()
        {
            var parser = new Args(new[] { "/sv=SQL100" });
            parser.Parse();
            
            Assert.AreEqual(SqlServerVersion.Sql100, parser.SqlServerVersion);
        }

        [Test]
        public void Can_Parse_This_Reference()
        {

            var parser = new Args(new[] { @"/r=this=c:\path\to\dacpac.dacpac=dacpacName" });
            parser.Parse();

            Assert.AreEqual(1, parser.References.Count);

            var thisReference = parser.References.FirstOrDefault() as ThisReference;
            Assert.IsNotNull(thisReference);
            Assert.True(thisReference.GetData().Items.Exists(p => p.Name == "FileName" && p.Value == @"c:\path\to\dacpac.dacpac"));
            Assert.True(thisReference.GetData().Items.Exists(p => p.Name == "LogicalName" && p.Value == "dacpacName"));

        }

        [Test]
        public void Can_Parse_Other_Reference()
        {

            var parser = new Args(new[] { @"/r=other=c:\path\to\dacpac.dacpac=dacpacName=dbName" });
            parser.Parse();

            Assert.AreEqual(1, parser.References.Count);

            var otherReference = parser.References.FirstOrDefault() as OtherReference;
            Assert.IsNotNull(otherReference);
            Assert.True(otherReference.GetData().Items.Exists(p => p.Name == "FileName" && p.Value == @"c:\path\to\dacpac.dacpac"));
            Assert.True(otherReference.GetData().Items.Exists(p => p.Name == "LogicalName" && p.Value == "dacpacName"));
            Assert.True(otherReference.GetData().Items.Exists(p => p.Name == "ExternalParts" && p.Value == "[$(dbName)]"));
            Assert.True(otherReference.GetData().RequiredSqlCmdVars.Exists(p=>p == "dbName"));

        }

        [Test]
        public void Can_Parse_Other_Server_Reference()
        {

            var parser = new Args(new[] { @"/r=otherserver=c:\path\to\dacpac.dacpac=dacpacName=dbName=serverName,123" });
            parser.Parse();

            Assert.AreEqual(1, parser.References.Count);

            var otherReference = parser.References.FirstOrDefault() as OtherServerReference;
            Assert.IsNotNull(otherReference);
            Assert.True(otherReference.GetData().Items.Exists(p => p.Name == "FileName" && p.Value == @"c:\path\to\dacpac.dacpac"));
            Assert.True(otherReference.GetData().Items.Exists(p => p.Name == "LogicalName" && p.Value == "dacpacName"));
            Assert.True(otherReference.GetData().Items.Exists(p => p.Name == "ExternalParts" && p.Value == "[$(serverName,123)].[$(dbName)]"));

            Assert.True(otherReference.GetData().RequiredSqlCmdVars.Exists(p => p == "dbName"));
            Assert.True(otherReference.GetData().RequiredSqlCmdVars.Exists(p => p == "serverName,123"));

        }

        [Test]
        public void Can_Parse_Master_Reference()
        {

            var parser = new Args(new[] { @"/r=MASTER=c:\path\to\dacpac.dacpac" });
            parser.Parse();

            Assert.AreEqual(1, parser.References.Count);

            var otherReference = parser.References.FirstOrDefault() as SystemReference;
            Assert.IsNotNull(otherReference);
            Assert.True(otherReference.GetData().Items.Exists(p => p.Name == "FileName" && p.Value == @"c:\path\to\dacpac.dacpac"));
            Assert.True(otherReference.GetData().Items.Exists(p => p.Name == "LogicalName" && p.Value == "master.dacpac"));
            Assert.True(otherReference.GetData().Items.Exists(p => p.Name == "ExternalParts" && p.Value == "[master]"));    //todo: need to verify master is not a cmdvar
            
        }


        [Test]
        public void Can_Parse_MSDB_Reference()
        {

            var parser = new Args(new[] { @"/r=msDB=c:\path\to\dacpac.dacpac" });
            parser.Parse();

            Assert.AreEqual(1, parser.References.Count);

            var otherReference = parser.References.FirstOrDefault() as SystemReference;
            Assert.IsNotNull(otherReference);
            Assert.True(otherReference.GetData().Items.Exists(p => p.Name == "FileName" && p.Value == @"c:\path\to\dacpac.dacpac"));
            Assert.True(otherReference.GetData().Items.Exists(p => p.Name == "LogicalName" && p.Value == "msdb.dacpac"));
            Assert.True(otherReference.GetData().Items.Exists(p => p.Name == "ExternalParts" && p.Value == "[msdb]"));    //todo: need to verify msdb is not a cmdvar

        }


    }


}
