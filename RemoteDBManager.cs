//using MySql.Data.MySqlClient;

//namespace Toto
//{
//    class RemoteDBManager
//    {
//        public static void Send(int tirazh, string betString)
//        {
//            using (var cn = new MySqlConnection(Properties.Settings.Default.MySqlConnection))
//            using (var cm = cn.CreateCommand())
//            {
//                cm.CommandText = "insert SKY_TOTO (coupon_number, bet_string) values (" + tirazh + ", '" + betString + "')";
//                cn.Open();
//                cm.ExecuteNonQuery();
//            }
//        }
//    }
//}
