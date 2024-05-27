using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Web.Helpers;

namespace LibMQTTDriver
{
    public class LibMqttDriver
    {
        public MqttClient client;
        protected string _broker_IPaddress = "192.168.1.101";

        public void Subcribe(string _topic)
        {
            string Topic = "";
            try
            {
                Topic = _topic + "#";
                client.Subscribe(new string[] { Topic }, new byte[] { 2 });   // we need arrays as parameters because we can subscribe to different topics with one call
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to subcribe Topic :\n" + Topic + "\n\n" + ex.ToString(), "Subcription", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Connect(string clientid = "broker123456", string _broker_IPaddress = "127.0.0.1", string username = "", string ServerPassword = "", byte willQOSLevel = 0)
        {
            try
            {
                //MessageBox.Show("Broker IPaddress connect = " + _broker_IPaddress);
                var state = 0;
                client = new MqttClient(_broker_IPaddress, 1883, false, MqttSslProtocols.None, null, null);

                if (username == "")
                    state = client.Connect(clientid);
                else
                    state = client.Connect(clientid, username, ServerPassword, false, willQOSLevel, false, null, null, true, 60);
            }
            catch (Exception ex)
            {
                string strInfo = string.Format("Fail to connect broker IP: {0}: {1}", _broker_IPaddress, ex.Message);
                MessageBox.Show("Connection to broker:\n\n{0}", strInfo, MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        public void Disconnect()
        {
            try
            {
                client.Disconnect();

                //base.Disconnect(e);
            }
            catch(Exception ex) { }            
        }

        public void DataPublish(string _topic, string jsonData, byte QOSLevel, bool retain)
        {
            try
            {
                //var jsonMessage = Json.Encode(jsonData);
                client.Publish(_topic, Encoding.UTF8.GetBytes(jsonData), QOSLevel, retain);
            }
            catch (Exception ex)
            {
                string strInfo = string.Format("Fail to publish topic {0} to broker: {1} \n\n {2}", _topic, jsonData, ex.Message);
                MessageBox.Show("Data Publishing :\n\n{0}\n\n" + ex.ToString(), strInfo, MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        public bool MQTT_Connected()
        {
            bool blnMQTTSTATUS;

            blnMQTTSTATUS = client.IsConnected; 

            return blnMQTTSTATUS;
        }

        public void DataPublish(string _topic, string jsonData)
        {
            try
            {
                //var jsonMessage = Json.Encode(jsonData);
                client.Publish(_topic, Encoding.UTF8.GetBytes(jsonData));
            }
            catch (Exception ex)
            {
                string strInfo = string.Format("Fail to publish topic {0} to broker: {1} \n\n {2}", _topic, jsonData, ex.Message);
                MessageBox.Show("Data Publishing :\n\n{0}\n\n" + ex.ToString(), strInfo, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public String GetTimestamp(DateTime value)
        {
            string strdatetime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", value);
            return strdatetime;
        }

        public string Handler_Address(string addr)
        {
            switch (addr)
            {
                case "0":
                    //Broker address 2
                    _broker_IPaddress = "127.0.0.1";
                    break;
                case "1":
                default:
                    //Broker address 1
                    _broker_IPaddress = "192.168.0.1";
                    break;

                case "2":
                    //Broker address 2
                    _broker_IPaddress = "192.168.0.2";
                    break;

                case "3":
                    //Broker address 3
                    _broker_IPaddress = "192.168.0.3";
                    break;

                case "4":
                    //Broker address 4
                    _broker_IPaddress = "192.168.0.4";
                    break;
            }

            return _broker_IPaddress;
        }

        #region EOT Item
        public class EOTmsg
        {
            public EOTItem EOT;
        }
        public class EOTItem
        {
            public int id { get; set; }
            public int bin { get; set; }
            public string timestamp { get; set; }
        }
        #endregion

        #region SOT information
        public class SOTmsg
        {
            public SOTItem _sot;
        }
        public class SOTItem
        {
            public int id { get; set; }
            public bool dutstat { get; set; }
            public double plunging_force_kgf { get; set; }
            public int arm_no { get; set; }
            public double ep_value_current { get; set; }
            public string unitID { get; set; }
            public double contactX { get; set; }
            public double contactY { get; set; }
            public double contactZ { get; set; }
            public double epoffset { get; set; }
            public double touchdowncount { get; set; }
            public double trayCoorX { get; set; }
            public double trayCoorY { get; set; }
            public double sot_delayms { get; set; }
            public string timestamp { get; set; }
        }
        #endregion

        #region TIP topic
        public class TIPmsg
        {
            public TestInProgressItem TIP;
        }
        public class TestInProgressItem
        {
            public int id { get; set; }
            public string timestamp { get; set; }
        }
        #endregion

        //#region LotInfo
        //public class Lot_info
        //{
        //    public LotinfoItem LotInfo; 
        //}

        //public class LotinfoItem
        //{
        //    public string packagename { get; set; }
        //    public string lotid { get; set; }
        //    public string sublotid { get; set; }
        //    public string operatorid { get; set; }
        //    public string mfgid { get; set; }
        //    public string handleid { get; set; }
        //    public bool alive { get; set; }
        //    public string timestamp { get; set; }

        //}
        //#endregion

        //#region UnitInfo
        //public class Unit_Info
        //{
        //    public UnitinfoItem UnitInfo;
        //}

        //public class UnitinfoItem
        //{
        //    public int pid { get; set; }
        //    public string handleid { get; set; }
        //    public double contactx { get; set; }
        //    public double contacty { get; set; }
        //    public double contactz { get; set; }
        //    public double handler_force_setting { get; set; }
        //    public double handler_ep_offset { get; set; }
        //    public int touchdown_count { get; set; }
        //    public double loadboard_temperature { get; set; }
        //    public bool timestamp { get; set; }

        //}
        //#endregion

        //#region UnitInfo
        //public class Test_Info
        //{
        //    public TestinfoItem TestInfo;
        //}

        //public class TestinfoItem
        //{
        //    public int pid { get; set; }
        //    public int site { get; set; }
        //    public int arm { get; set; }
        //    public string TestStatus { get; set; }
        //    public double siteyield { get; set; }
        //    public double armyield { get; set; }
        //    public Dictionary<string,double> top_reject_parameter { get; set; }
        //    public double test_time { get; set; }
        //    public bool timestamp { get; set; }

        //}
        ////public class TopRejectParameterList
        ////{
        ////    public Dictionary<string, double> toprejectparameter { get; set; }
        ////}
        //#endregion
    }
}
