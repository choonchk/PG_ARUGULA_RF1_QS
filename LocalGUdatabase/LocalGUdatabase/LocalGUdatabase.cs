using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data;
using System.Data.SQLite;

namespace LocalGUdatabase
{
    public static class GUconstants
    {
        public const string directoryDB = @"C:\Avago.ATF.Common.x64\LocalGU\";
        public const string fileDB = @"C:\Avago.ATF.Common.x64\LocalGU\LocalGU.db";
        public const string connection = "data source=\"" + "C:\\Avago.ATF.Common.x64\\LocalGU\\LocalGU.db\"";
    }

    public class GUsqlite
    {
        string directoryDB = GUconstants.directoryDB;
        string fileDB = GUconstants.fileDB;
        string connection = GUconstants.connection;
        static GUsqlite _instance;

        GUWriter _gu_writer = new GUWriter();
        //GUReader _gu_reader = new GUReader();


        public enum GUType : int
        {
            GUVrfy = 0,
            GUCorrVrfy = 1
        }

        public enum CFType : int
        {
            Add = 0,
            Multiply = 1
        }

        public static GUsqlite Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GUsqlite();
                }

                return _instance;
            }
        }

        public GUWriter GUwriter
        {
            get
            {
                return _gu_writer;
            }
        }

        //public GUReader GUreader
        //{
        //    get
        //    {
        //        return _gu_reader;
        //    }
        //}

        public GUsqlite()
        {
            string[] sqlCmds = new string[13];

            sqlCmds[0] =
                "CREATE TABLE IF NOT EXISTS gu_data_list(" +
                    "gu_attempt_index integer NOT NULL DEFAULT 0 PRIMARY KEY AUTOINCREMENT," +
                    "gu_product_tag       text NOT NULL," +
                    "attempt_datetime datetime  DEFAULT CURRENT_DATETIME" +
                ")";

            sqlCmds[1] =
                "CREATE TABLE IF NOT EXISTS gu_statistics(" +
                    "param_header         text NOT NULL," +
                    "gu_product_tag       text NOT NULL," +
                    "calculated_cf_upper_limit double NOT NULL," +
                    "calculated_cf_lower_limit double NOT NULL," +
                    "calculated_ve_upper_limit double," +
                    "calculated_ve_lower_limit double," +
                    "revision             integer NOT NULL DEFAULT 0" +
                ")";

            sqlCmds[2] =
                "CREATE TABLE IF NOT EXISTS gu_summary(" +
                    "gu_attempt_index     integer NOT NULL DEFAULT 0," +
                    "package_name         text NOT NULL," +
                    "gucal_filename       text NOT NULL," +
                    "tester_name               text NOT NULL DEFAULT 'NA'," +
                    "handler_name             text NOT NULL DEFAULT 'NA'," +
                    "site_no                   integer NOT NULL DEFAULT 1," +
                    "total_param_count    integer NOT NULL DEFAULT 1," +
                    "attempt_count        integer NOT NULL DEFAULT 1," +
                    "gu_type              integer NOT NULL DEFAULT 0," +
                    "FOREIGN KEY(gu_attempt_index) REFERENCES gu_data_list(gu_attempt_index) ON DELETE CASCADE" +
                ")";

            sqlCmds[3] =
                "CREATE TABLE IF NOT EXISTS gu_verify_summary(" +
                    "gu_attempt_index     integer NOT NULL DEFAULT 0," +
                    "gu_id                integer NOT NULL DEFAULT - 1," +
                    "gu_batch             integer NOT NULL DEFAULT - 1," +
                    "failed_param_count   integer NOT NULL DEFAULT 0," +
                    "verify_status        bit  DEFAULT FALSE," +
                    "attempt_datetime     datetime DEFAULT CURRENT_DATETIME," +
                    "FOREIGN KEY(gu_attempt_index) REFERENCES gu_data_list(gu_attempt_index) ON DELETE CASCADE" +
                ")";

            sqlCmds[4] =
                "CREATE TABLE IF NOT EXISTS gu_corr_summary(" +
                    "gu_attempt_index     integer NOT NULL DEFAULT 0," +
                    "gu_batch             integer NOT NULL DEFAULT - 1," +
                    "failed_param_count   integer NOT NULL DEFAULT 0," +
                    "corr_status          bit NOT NULL DEFAULT FALSE," +
                    "FOREIGN KEY(gu_attempt_index) REFERENCES gu_data_list(gu_attempt_index) ON DELETE CASCADE" +
                ")";

            sqlCmds[5] =
                "CREATE TABLE IF NOT EXISTS gu_pareto(" +
                    "gu_attempt_index     integer NOT NULL DEFAULT 0," +
                    "gu_id                integer NOT NULL DEFAULT - 1," +
                    "failed_param_count   integer NOT NULL DEFAULT 0," +
                    "FOREIGN KEY(gu_attempt_index) REFERENCES gu_data_list(gu_attempt_index) ON DELETE CASCADE" +
                ")";

            sqlCmds[6] =
                "CREATE TABLE IF NOT EXISTS gu_verify_raw_data(" +
                    "gu_attempt_index     integer NOT NULL DEFAULT 0," +
                    "param_number         integer NOT NULL DEFAULT 0," +
                    "param_header         text NOT NULL," +
                    "value                double NOT NULL," +
                    "error_upper_limit    double NOT NULL," +
                    "error_lower_limit    double NOT NULL," +
                    "gu_id                integer NOT NULL DEFAULT - 1," +
                    "ref_value            double," +
                    "cf_value             double," +
                    "verify_error         double NOT NULL," +
                    "status               bit NOT NULL DEFAULT FALSE," +
                    "FOREIGN KEY(gu_attempt_index) REFERENCES gu_data_list(gu_attempt_index) ON DELETE CASCADE" +
                ")";

            sqlCmds[7] =
                "CREATE TABLE IF NOT EXISTS gu_corr_factor(" +
                    "gu_attempt_index     integer NOT NULL DEFAULT 0," +
                    "cf_value             double," +
                    "status               bit NOT NULL DEFAULT FALSE," +
                    "param_header         text NOT NULL," +
                    "multiply             bit NOT NULL DEFAULT FALSE," +
                    "addition             bit NOT NULL DEFAULT FALSE," +
                    "FOREIGN KEY(gu_attempt_index) REFERENCES gu_data_list(gu_attempt_index) ON DELETE CASCADE" +
                ")";

            sqlCmds[8] =
                "CREATE TABLE IF NOT EXISTS gu_corr_raw_data(" +
                    "gu_attempt_index     integer NOT NULL DEFAULT 0," +
                    "param_number         integer NOT NULL DEFAULT 0," +
                    "param_header         text NOT NULL," +
                    "value                double NOT NULL," +
                    "cf_upper_limit       double DEFAULT 9999999," +
                    "cf_lower_limit       double DEFAULT -9999999," +
                    "gu_id                integer NOT NULL DEFAULT - 1," +
                    "ref_value            double," +
                    "status               bit NOT NULL DEFAULT FALSE," +
                    "FOREIGN KEY(gu_attempt_index) REFERENCES gu_data_list(gu_attempt_index) ON DELETE CASCADE" +
                ")";

            sqlCmds[9] = "CREATE UNIQUE INDEX IF NOT EXISTS unq_gu_summary_gu_attempt_index ON gu_summary(gu_attempt_index )";

            sqlCmds[10] = "CREATE UNIQUE INDEX IF NOT EXISTS unq_gu_verify_summary_gu_attempt_index ON gu_verify_summary(gu_attempt_index, gu_id, gu_batch)";

            sqlCmds[11] = "CREATE UNIQUE INDEX IF NOT EXISTS unq_gu_corr_summary_gu_attempt_index ON gu_corr_summary(gu_attempt_index, gu_batch)";

            sqlCmds[12] = "CREATE UNIQUE INDEX IF NOT EXISTS unq_gu_statistics_param ON gu_statistics(gu_product_tag, param_header)";

            try
            {
                if (Directory.Exists(directoryDB) == false)
                {
                    Directory.CreateDirectory(directoryDB);
                }
                if (Directory.Exists(directoryDB) == true)
                {
                    try
                    {
                        for (int i = 0; i < sqlCmds.Length; i++)
                        {
                            using (SQLiteConnection conn = new SQLiteConnection(connection))
                            {
                                conn.Open();
                                SQLiteCommand cmd = new SQLiteCommand(conn);

                                cmd.CommandText = sqlCmds[i];
                                cmd.CommandType = CommandType.Text;
                                SQLiteDataReader reader = cmd.ExecuteReader();

                                conn.Close();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Print("GUsqlite(): " + ex.Message);
                    }

                    // delete records older than a week
                    DeleteOldRecords();
                }
            }
            catch (Exception ex)
            {
                Debug.Print("GUsqlite() :" + ex.Message);
            }
        }

        void DeleteOldRecords()
        {
            string sqlCmd = "DELETE FROM gu_data_list WHERE attempt_datetime < @start_date";


            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connection))
                {
                    SQLiteTransaction transaction = null;
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    SQLiteCommand cmd = new SQLiteCommand(conn);

                    cmd.CommandText = sqlCmd;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@start_date", DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0)).ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.Print("DeleteOldRecords: " + ex.Message);
            }
        }

        public class GUWriter
        {
            string directoryDB = GUconstants.directoryDB;
            string fileDB = GUconstants.fileDB;
            string connection = GUconstants.connection;
            string sqlCmd = "";

            SQLiteConnection conn;
            SQLiteTransaction transaction;
            SQLiteCommand cmd;
            SQLiteDataReader reader;

            long last_gu_attempt_index = 0;
            public long LastGUAttemptIndex
            {
                get
                {
                    return last_gu_attempt_index;
                }
            }
            public void OpenDB()
            {
                conn = new SQLiteConnection(connection);
                transaction = null;

                conn.Open();
                transaction = conn.BeginTransaction();

                cmd = new SQLiteCommand(conn);
                //cmd.CommandText = sqlCmd;
                cmd.CommandType = CommandType.Text;


            }
            public void Commit()
            {

                transaction.Commit();
                conn.Close();
                //success = true;
            }
            public long GenerateNewGUattempt(string product_tag)
            {
                long rowID = 0;
                sqlCmd = "INSERT INTO gu_data_list(gu_product_tag, attempt_datetime) VALUES(@product_tag, @datetime)";

                try
                {
                    //using (SQLiteConnection conn = new SQLiteConnection(connection))
                    //{
                    //    SQLiteTransaction transaction = null;
                    //    conn.Open();
                    //    transaction = conn.BeginTransaction();

                    //    SQLiteCommand cmd = new SQLiteCommand(conn);

                    cmd.CommandText = sqlCmd;
                    //cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@product_tag", product_tag);
                    cmd.Parameters.AddWithValue("@datetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();

                    //reader = cmd.ExecuteReader();

                    last_gu_attempt_index = rowID = conn.LastInsertRowId;
                    //reader.Close();
                    //transaction.Commit();
                    //conn.Close();
                    //   }
                }
                catch (Exception ex)
                {
                    rowID = 0;
                    Debug.Print("GenerateNewGUattempt: " + ex.Message);
                }

                return rowID;
            }

            // package_name : AFEM-8230-AP1-RF1_BE-PXI-NI_v0012
            // gucal_filename: IP172.16.7.149_2022-05-08_17.38.56_ACFM-WH13-AP1-RF1_BE-ZNB_V0028_GuCorrVrfy_PF_WIML8I6.gucal
            // total_param_count : TOTAL_TESTS
            // attempt_count : <CorrelationFailures>9</CorrelationFailures>
            // tester_name : computer name
            // handler_name : SJHandlerSim1Site02
            // product_tag  : product_tag

            public bool InsertGUSummary(string package_name, string gucal_filename,
                int total_param_count, int attempt_count, GUType gu_type,
                string tester_name, string handler_name, int site_no)
            {
                bool success = false;

                sqlCmd = "INSERT OR REPLACE INTO gu_summary(gu_attempt_index, package_name, gucal_filename, total_param_count, attempt_count, gu_type, tester_name, handler_name, site_no) " +
                                "VALUES(@gu_attempt_index, @package_name, @gucal_filename, @total_param_count, @attempt_count, @gu_type, @tester_name, @handler_name, @site_no)";

                try
                {
                    //using (SQLiteConnection conn = new SQLiteConnection(connection))
                    //{
                    //SQLiteTransaction transaction = null;
                    //conn.Open();
                    //transaction = conn.BeginTransaction();

                    //SQLiteCommand cmd = new SQLiteCommand(conn);

                    cmd.CommandText = sqlCmd;
                    //cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@gu_attempt_index", last_gu_attempt_index);
                    cmd.Parameters.AddWithValue("@package_name", package_name);
                    cmd.Parameters.AddWithValue("@gucal_filename", gucal_filename);
                    cmd.Parameters.AddWithValue("@total_param_count", total_param_count);
                    cmd.Parameters.AddWithValue("@attempt_count", attempt_count);
                    cmd.Parameters.AddWithValue("@gu_type", (int)gu_type);
                    cmd.Parameters.AddWithValue("@tester_name", tester_name);
                    cmd.Parameters.AddWithValue("@handler_name", handler_name);
                    cmd.Parameters.AddWithValue("@site_no", site_no);
                    //     reader = cmd.ExecuteReader();

                    //transaction.Commit();
                    //conn.Close();
                    success = true;
                    cmd.ExecuteNonQuery();
                    //}
                }
                catch (Exception ex)
                {
                    Debug.Print("InsertGUSummary: " + ex.Message);
                }
                return success;
            }

            public bool InsertGUCorrSummary(int gu_batch, int failed_param_count, bool corr_status)
            {
                bool success = false;

                sqlCmd = "INSERT OR REPLACE INTO gu_corr_summary(gu_attempt_index, gu_batch, failed_param_count, corr_status) " +
                                "VALUES(@gu_attempt_index, @gu_batch, @failed_param_count, @corr_status)";

                try
                {
                    //using (SQLiteConnection conn = new SQLiteConnection(connection))
                    //{
                    //    SQLiteTransaction transaction = null;
                    //    conn.Open();
                    //    transaction = conn.BeginTransaction();

                    //    SQLiteCommand cmd = new SQLiteCommand(conn);

                    cmd.CommandText = sqlCmd;
                    //cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@gu_attempt_index", last_gu_attempt_index);
                    cmd.Parameters.AddWithValue("@gu_batch", gu_batch);
                    cmd.Parameters.AddWithValue("@failed_param_count", failed_param_count);
                    cmd.Parameters.AddWithValue("@corr_status", corr_status);
                    //     reader = cmd.ExecuteReader();

                    //transaction.Commit();
                    //conn.Close();
                    success = true;
                    cmd.ExecuteNonQuery();
                    //}
                }
                catch (Exception ex)
                {
                    Debug.Print("InsertGUCorrSummary: " + ex.Message);
                }
                return success;
            }

            public bool InsertGuCorrFactor(double cf_value, bool status, string param_header, CFType cf_type)
            {
                bool success = false;

                sqlCmd = "INSERT INTO gu_corr_factor(gu_attempt_index, cf_value, status, param_header, multiply, addition) " +
                                "VALUES(@gu_attempt_index, @cf_value, @status, @param_header, @multiply, @addition)";

                try
                {
                    //using (SQLiteConnection conn = new SQLiteConnection(connection))
                    //{
                    //    SQLiteTransaction transaction = null;
                    //    conn.Open();
                    //    transaction = conn.BeginTransaction();

                    //    SQLiteCommand cmd = new SQLiteCommand(conn);

                    cmd.CommandText = sqlCmd;
                    //cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@gu_attempt_index", last_gu_attempt_index);
                    cmd.Parameters.AddWithValue("@cf_value", cf_value);
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@param_header", param_header);
                    cmd.Parameters.AddWithValue("@multiply", cf_type == CFType.Multiply ? true : false);
                    cmd.Parameters.AddWithValue("@addition", cf_type == CFType.Add ? true : false);
                    //      reader = cmd.ExecuteReader();

                    //transaction.Commit();
                    //conn.Close();
                    success = true;
                    cmd.ExecuteNonQuery();

                    //}
                }
                catch (Exception ex)
                {
                    Debug.Print("InsertGuCorrFactor: " + ex.Message);
                }

                return success;
            }

            public bool InsertGuCorrRawData(int param_number, string param_header, double value, double cf_upper_limit, double cf_lower_limit, int gu_id, double ref_value, bool status)
            {
                bool success = false;

                sqlCmd = "INSERT INTO gu_corr_raw_data(gu_attempt_index, param_number, param_header, value, cf_upper_limit, cf_lower_limit, gu_id, ref_value, status) " +
                                "VALUES(@gu_attempt_index, @param_number, @param_header, @value, @cf_upper_limit, @cf_lower_limit, @gu_id, @ref_value, @status)";

                try
                {
                    //using (SQLiteConnection conn = new SQLiteConnection(connection))
                    //{
                    //    SQLiteTransaction transaction = null;
                    //    conn.Open();
                    //    transaction = conn.BeginTransaction();

                    //    SQLiteCommand cmd = new SQLiteCommand(conn);

                    cmd.CommandText = sqlCmd;
                    //cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@gu_attempt_index", last_gu_attempt_index);
                    cmd.Parameters.AddWithValue("@param_number", param_number);
                    cmd.Parameters.AddWithValue("@param_header", param_header);
                    cmd.Parameters.AddWithValue("@value", value);
                    cmd.Parameters.AddWithValue("@cf_upper_limit", cf_upper_limit);
                    cmd.Parameters.AddWithValue("@cf_lower_limit", cf_lower_limit);
                    cmd.Parameters.AddWithValue("@gu_id", gu_id);
                    cmd.Parameters.AddWithValue("@ref_value", ref_value);
                    cmd.Parameters.AddWithValue("@status", status);
                    //     reader = cmd.ExecuteReader();

                    //transaction.Commit();
                    //conn.Close();
                    success = true;
                    cmd.ExecuteNonQuery();
                    //}
                }
                catch (Exception ex)
                {
                    Debug.Print("InsertGuCorrRawData: " + ex.Message);
                }

                return success;
            }

            public bool InsertGuVerifySummary(int gu_id, int gu_batch, int failed_param_count, bool verify_status)
            {
                bool success = false;

                sqlCmd = "INSERT OR REPLACE INTO gu_verify_summary(gu_attempt_index, gu_id, gu_batch, failed_param_count, verify_status, attempt_datetime) " +
                                "VALUES(@gu_attempt_index, @gu_id, @gu_batch, @failed_param_count, @verify_status, @datetime)";

                //sqlCmd = "INSERT INTO gu_verify_summary(gu_attempt_index, gu_id, gu_batch, failed_param_count, verify_status) " +
                //                "VALUES(@gu_attempt_index, @gu_id, @gu_batch, @failed_param_count, @verify_status)";
                try
                {
                    //using (SQLiteConnection conn = new SQLiteConnection(connection))
                    //{
                    //    SQLiteTransaction transaction = null;
                    //    conn.Open();
                    //    transaction = conn.BeginTransaction();

                    //     SQLiteCommand cmd = new SQLiteCommand(conn);

                    cmd.CommandText = sqlCmd;
                    //cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@gu_attempt_index", last_gu_attempt_index);
                    cmd.Parameters.AddWithValue("@gu_id", gu_id);
                    cmd.Parameters.AddWithValue("@gu_batch", gu_batch);
                    cmd.Parameters.AddWithValue("@failed_param_count", failed_param_count);
                    cmd.Parameters.AddWithValue("@verify_status", verify_status);
                    cmd.Parameters.AddWithValue("@datetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    //     reader = cmd.ExecuteReader();

                    //    transaction.Commit();
                    //    conn.Close();
                    success = true;
                    cmd.ExecuteNonQuery();
                    //}
                }
                catch (Exception ex)
                {
                    Debug.Print("InsertGuVerifySummary: " + ex.Message);
                }

                return success;
            }

            public bool InserGuVerifyRawData(int param_number, string param_header, double value, double error_upper_limit, double error_lower_limit, int gu_id, double ref_value, double cf_value, double verify_error, bool status)
            {
                bool success = false;

                sqlCmd = "INSERT INTO gu_verify_raw_data(gu_attempt_index, param_number, param_header, value, error_upper_limit, error_lower_limit, gu_id, ref_value, cf_value, verify_error, status) " +
                                "VALUES(@gu_attempt_index, @param_number, @param_header, @value, @error_upper_limit, @error_lower_limit, @gu_id, @ref_value, @cf_value, @verify_error, @status)";

                try
                {
                    //using (SQLiteConnection conn = new SQLiteConnection(connection))
                    //{
                    //    SQLiteTransaction transaction = null;
                    //    conn.Open();
                    //    transaction = conn.BeginTransaction();

                    //    SQLiteCommand cmd = new SQLiteCommand(conn);

                    cmd.CommandText = sqlCmd;
                    //cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@gu_attempt_index", last_gu_attempt_index);
                    cmd.Parameters.AddWithValue("@param_number", param_number);
                    cmd.Parameters.AddWithValue("@param_header", param_header);
                    cmd.Parameters.AddWithValue("@value", value);
                    cmd.Parameters.AddWithValue("@error_upper_limit", error_upper_limit);
                    cmd.Parameters.AddWithValue("@error_lower_limit", error_lower_limit);
                    cmd.Parameters.AddWithValue("@gu_id", gu_id);
                    cmd.Parameters.AddWithValue("@ref_value", ref_value);
                    cmd.Parameters.AddWithValue("@cf_value", cf_value);
                    cmd.Parameters.AddWithValue("@verify_error", verify_error);
                    cmd.Parameters.AddWithValue("@status", status);
                    //     reader = cmd.ExecuteReader();

                    //transaction.Commit();
                    //conn.Close();
                    success = true;
                    cmd.ExecuteNonQuery();
                    // }
                }
                catch (Exception ex)
                {
                    Debug.Print("InserGuVerifyRawData: " + ex.Message);
                }

                return success;
            }

            public bool InsertGuPareto(int gu_id, int failed_param_count)
            {
                bool success = false;

                sqlCmd = "INSERT INTO gu_pareto(gu_attempt_index, gu_id, failed_param_count) " +
                                "VALUES(@gu_attempt_index, @gu_id, @failed_param_count)";

                try
                {
                    //using (SQLiteConnection conn = new SQLiteConnection(connection))
                    //{
                    //    SQLiteTransaction transaction = null;
                    //    conn.Open();
                    //    transaction = conn.BeginTransaction();

                    //    SQLiteCommand cmd = new SQLiteCommand(conn);

                    cmd.CommandText = sqlCmd;
                    //cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@gu_attempt_index", last_gu_attempt_index);
                    cmd.Parameters.AddWithValue("@gu_id", gu_id);
                    cmd.Parameters.AddWithValue("@failed_param_count", failed_param_count);
                    //    reader = cmd.ExecuteReader();

                    //transaction.Commit();
                    //conn.Close();
                    success = true;
                    cmd.ExecuteNonQuery();
                    //}
                }
                catch (Exception ex)
                {
                    Debug.Print("InsertGuPareto: " + ex.Message);
                }

                return success;
            }

            public bool UpdateGuStatistics(string param_header, string product_tag, double calculated_cf_upper_limit, double calculated_cf_lower_limit, double calculated_ve_upper_limit, double calculated_ve_lower_limit, int revision)
            {
                bool success = false;

                sqlCmd = "INSERT OR REPLACE INTO gu_statistics(param_header, gu_product_tag, calculated_cf_upper_limit, calculated_cf_lower_limit, calculated_ve_upper_limit, calculated_ve_lower_limit, revision) " +
                                "VALUES(@param_header, @gu_product_tag, @calculated_cf_upper_limit, @calculated_cf_lower_limit, @calculated_ve_upper_limit, @calculated_ve_lower_limit, @revision)";

                try
                {
                    //using (SQLiteConnection conn = new SQLiteConnection(connection))
                    //{
                    //    SQLiteTransaction transaction = null;
                    //    conn.Open();
                    //    transaction = conn.BeginTransaction();

                    //    SQLiteCommand cmd = new SQLiteCommand(conn);

                    cmd.CommandText = sqlCmd;
                    //cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@param_header", param_header);
                    cmd.Parameters.AddWithValue("@gu_product_tag", product_tag);
                    cmd.Parameters.AddWithValue("@calculated_cf_upper_limit", calculated_cf_upper_limit);
                    cmd.Parameters.AddWithValue("@calculated_cf_lower_limit", calculated_cf_lower_limit);
                    cmd.Parameters.AddWithValue("@calculated_ve_upper_limit", calculated_ve_upper_limit);
                    cmd.Parameters.AddWithValue("@calculated_ve_lower_limit", calculated_ve_lower_limit);
                    cmd.Parameters.AddWithValue("@revision", revision);
                    //     reader = cmd.ExecuteReader();

                    //transaction.Commit();
                    //conn.Close();
                    success = true;
                    cmd.ExecuteNonQuery();
                    //}
                }
                catch (Exception ex)
                {
                    Debug.Print("UpdateGuStatistics: " + ex.Message);
                }

                return success;
            }
        }


        public class GUReader
        {
            string directoryDB = GUconstants.directoryDB;
            string fileDB = GUconstants.fileDB;
            string connection = GUconstants.connection;

            public DataTable QueryDataFromDB(string sql_cmd)
            {
                DataTable tblResults = new DataTable();
                SQLiteDataAdapter adapter;

                try
                {
                    string sqlCmd = sql_cmd.Trim(new char[] { ' ', ',', '\'', ';', '.' });

                    using (SQLiteConnection conn = new SQLiteConnection(connection))
                    {
                        SQLiteTransaction transaction = null;
                        conn.Open();

                        transaction = conn.BeginTransaction();

                        SQLiteCommand cmd = new SQLiteCommand(conn);


                        cmd.CommandText = sqlCmd;
                        cmd.CommandType = CommandType.Text;

                        adapter = new SQLiteDataAdapter(cmd);
                        adapter.Fill(tblResults);


                        transaction.Commit();
                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print("QueryDataFromDB() :" + ex.Message);
                }

                return tblResults;
            }

            //    private List<GU_Summary> GetOneWeekGUSummary(string gu_product_tag = "")
            //    {
            //        List<GU_Summary> summaryList = new List<GU_Summary>();
            //        DataTable tblResult = new DataTable();
            //        SQLiteDataAdapter adapter;

            //        try
            //        {
            //            string sqlCmd = string.Empty;

            //            using (SQLiteConnection conn = new SQLiteConnection(connection))
            //            {
            //                SQLiteTransaction transaction = null;
            //                conn.Open();

            //                transaction = conn.BeginTransaction();

            //                SQLiteCommand cmd = new SQLiteCommand(conn);

            //                sqlCmd = string.Format("SELECT `gu_product_tag`,`gu_attempt_index`,`attempt_datetime`, `package_name`, `gucal_filename`, `total_param_count`, `attempt_count`, `gu_type`, `tester_name`, `handler_name`, `site_no` " + 
            //                    "FROM `gu_data_list` as A INNER JOIN `gu_summary` AS B ON A.gu_attempt_index = B.gu_attempt_index "+ 
            //                    "WHERE A.`gu_product_tag`='{0}'  AND A.`attempt_datetime`>='{1:yyyy-MM-dd 00:00:00}' AND `attempt_datetime`<='{2:yyyy-MM-dd 23:59:59}' order by `attempt_datetime` desc",
            //                    gu_product_tag, DateTime.Today.Subtract(new TimeSpan(7, 0, 0, 0)), DateTime.Today);
            //                cmd.CommandText = sqlCmd;
            //                cmd.CommandType = CommandType.Text;

            //                adapter = new SQLiteDataAdapter(cmd);
            //                adapter.Fill(tblResult);

            //                foreach (DataRow r in tblResult.Rows)
            //                {
            //                    summaryList.Add(new GU_Summary(gu_product_tag, long.Parse(r[0].ToString()), DateTime.Parse(r[1].ToString())));
            //                }

            //                transaction.Commit();
            //                conn.Close();
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            Debug.Print("GetOneWeekGUAttempts() :" + ex.Message);
            //        }

            //        return summaryList;
            //    }
        }


        public class GU_Summary
        {
            private string _product_tag = string.Empty;
            private long _search_index = 0;
            private DateTime _timestamp = DateTime.MinValue;
            public string PackageName = string.Empty;
            public string GUCalFileName = string.Empty;
            public int TotalParameterCount = 0;
            public int AttemptNumber = 0;
            public GUType GUtype = GUType.GUVrfy;
            public string TesterName = string.Empty;
            public string HandlerName = string.Empty;
            public int SiteNumber = 1;
            public GU_Corr_Summary GUCorrelationSummary = new GU_Corr_Summary();
            public GU_Verify_Summary GUVerifySumary = new GU_Verify_Summary();


            public string ProductTag
            {
                get
                {
                    return _product_tag;
                }
            }

            public long SearchIndex
            {
                get
                {
                    return _search_index;
                }
            }

            public DateTime Timestamp
            {
                get
                {
                    return _timestamp;
                }
            }

            public GU_Summary(string product_tag, long attempt_index, DateTime timestamp)
            {
                _product_tag = product_tag;
                _search_index = attempt_index;
                _timestamp = timestamp;
            }


            public class GU_Corr_Summary
            {
                public int GUBatch = 0;
                public int FailedParameterCount = 0;
                public bool CorrelationPassed = false;
                public List<GU_Corr_Factor> CorrFactors = new List<GU_Corr_Factor>();
            }

            public class GU_Verify_Summary
            {
                public int GU_ID = 0;
                public int GUBatch = 0;
                public int FailedParameterCount = 0;
                public bool VerifyPassed = false;
                public List<GU_Verify_Pareto> VerifyPareto = new List<GU_Verify_Pareto>();
            }

            public class GU_Corr_Factor
            {
                public double CFvalue = 0;
                public bool Passed = false;
                public string ParameterHeader = string.Empty;
                public CFType Type = CFType.Add;
            }

            public class GU_Corr_Raw
            {
                public string ParameterHeader = string.Empty;
                public double Value = 0;
                public double UpperLimit = 0;
                public double LowerLimit = 0;
                public int GU_ID = 0;
                public double ReferenceValue = 0;
                public bool Passed = false;
            }

            public class GU_Verify_Pareto
            {
                public int GU_ID = 0;
                public int FailedParameterCount = 0;
            }

            public class GU_Verify_Raw
            {
                public string ParameterHeader = string.Empty;
                public double Value = 0;
                public double UpperLimit = 0;
                public double LowerLimit = 0;
                public int GU_ID = 0;
                public double ReferenceValue = 0;
                public double CFvalue = 0;
                public double VerifyError = 0;
                public bool Passed = false;
            }
        }


    }
}
