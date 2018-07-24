using System.Threading.Tasks;

namespace stocks
{
    public abstract class DbObjectBase
    {
        private string _server = "";
        private string _port = "3306";
        private string _userId = "";
        private string _password = "";
        private string _database = "";

        public string Database
        {
            get { return _database; }
            set { _database = value; }
        }

        public abstract string TableName { get; set; }

        public abstract string UpdateQuery { get; }

        public abstract string SelectQuery { get; }

        public string ConnectionString 
        {
            get
            {
                return $"Server={_server};Port={_port};Database={_database};Uid={_userId};Pwd={_password};";
            }
        }

        /// <summary>
        /// Save the object to the database
        /// </summary>
        /// <returns></returns>
        // public async Task Commit()
        // {
        //     using (var mySqlConnection = new MySqlConnection(ConnectionString))
        //     {
        //         using (var command = new MySqlCommand(UpdateQuery()))
        //         {
                    
        //         }
        //     }
        // }
    }
}