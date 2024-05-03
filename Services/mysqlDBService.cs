using bytech_boards.Models;
using MySql.Data.MySqlClient;
using System.Data;
using System.Reflection;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace bytech_boards.Services
{
    public class mysqlDBService : IDBService
    {
        private string _connectionString {  get; set; }
        private MySqlConnection _connection{  get; set; }

        private MySqlConnection GetConnection()
        {
            return _connection;
        }

        private async Task OpenConnection()
        {
            Console.WriteLine(_connection.ConnectionString);

            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                await _connection.OpenAsync();
            }
        }

        private async Task CloseConnection()
        {
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                await _connection.CloseAsync();
            }
        }

        public mysqlDBService(string connectionString)
        {
            _connectionString = connectionString;

            _connection = new MySqlConnection(connectionString);
        }

        async public Task AddData(string tableName, IModel data)
        {
            StringBuilder addDataQuery = new StringBuilder($"INSERT INTO {tableName}(");

            var properties = data.GetType().GetProperties().ToList();
            for (int i = 0; i < properties.Count; i++)
            {
                addDataQuery.Append(properties[i].Name.ToLower());
                if(i != properties.Count - 1)
                {
                    addDataQuery.Append(",");
                }
            }
            addDataQuery.Append(") VALUES(");
            for (int i = 0; i < properties.Count; i++)
            {
                addDataQuery.Append($"@value{i}");
                if (i != properties.Count - 1)
                {
                    addDataQuery.Append(",");
                }
            }

            addDataQuery.Append(")");

            MySqlCommand command = new MySqlCommand(addDataQuery.ToString(), _connection);

            addDataQuery.Append(")");
            for (int i = 0; i < properties.Count; i++)
            {
                command.Parameters.Add($"@value{i}", ConvertTypeToMysql(properties[i].PropertyType)).
                                                    Value = properties[i].GetValue(data);
            }

            await OpenConnection();

            await command.ExecuteNonQueryAsync();

            await CloseConnection();
        }

        async public Task DeleteData(string tableName, IModel model)
        {
            //  Основной строка запроса
            StringBuilder getDataQuery = new StringBuilder($"DELETE FROM {tableName} WHERE id = @modelId");
            //  Команда запроса
            MySqlCommand command = new MySqlCommand(getDataQuery.ToString(), GetConnection());

            command.Parameters.Add($"@modelId", ConvertTypeToMysql(model.Id.GetType())).Value = model.Id;

            await OpenConnection();

            await command.ExecuteNonQueryAsync();

            await CloseConnection();
        }

        async public Task EditData(IModel model, string tableName)
        {
            if (model == null || model.Id == null) { return; }

            //  Основной строка запроса
            StringBuilder getDataQuery = new StringBuilder($"SELECT * FROM {tableName} WHERE id = @modelId");
            //  Команда запроса
            MySqlCommand command = new MySqlCommand(getDataQuery.ToString(), GetConnection());

            command.Parameters.Add($"@modelId", ConvertTypeToMysql(model.Id.GetType())).Value = model.Id;

            //  Адаптер данных
            MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            //  Таблица данных из БД
            DataTable dataTable = new DataTable();

            await adapter.FillAsync(dataTable);

            if (dataTable.Rows.Count <= 0) { return; }

            getDataQuery = new StringBuilder($"UPDATE {tableName} SET ");

            List<PropertyInfo> properties = model.GetType().GetProperties().ToList();

            for (int i = 0; i < properties.Count; i++)
            {
                if (properties[i].Name.ToLower() == "id") { properties.RemoveAt(i); }

                getDataQuery.Append($"{properties[i].Name} = @paramValue{i}");
                if (i + 1 != properties.Count) { getDataQuery.Append(","); }
            }

            getDataQuery.Append($" WHERE id = @modelId");

            command = new MySqlCommand(getDataQuery.ToString(), GetConnection());

            command.Parameters.Add($"@modelId", ConvertTypeToMysql(model.Id.GetType())).Value = model.Id;

            for (int i = 0; i < properties.Count; i++)
            {
                command.Parameters.Add($"@paramValue{i}", ConvertTypeToMysql(properties[i].PropertyType)).Value = model.GetType().GetProperty(properties[i].Name).GetValue(model);
            }

            await OpenConnection();

            await command.ExecuteNonQueryAsync();

            await CloseConnection();
        }

        async public Task<List<TModel>> GetData<TModel>(string tableName, string condition = null) where TModel : IModel
        {
            //  Основной строка запроса
            StringBuilder getDataQuery = new StringBuilder($"SELECT * FROM {tableName} ");
            //  Команда запроса
            MySqlCommand command = new MySqlCommand();
            //  Адаптер данных
            MySqlDataAdapter adapter;
            //  Таблица данных из БД
            DataTable dataTable = new DataTable();

            Type type = typeof(TModel);

            if(condition != null)
            {
                getDataQuery.Append($"WHERE {condition}");
            }

            command = new MySqlCommand(getDataQuery.ToString(), GetConnection());

            //command.Parameters.Add("@condition", MySqlDbType.VarChar).Value = condition;

            adapter = new MySqlDataAdapter(command);

            await adapter.FillAsync(dataTable);

            List<IModel> models = new List<IModel>();

            var properties = type.GetProperties();

            int i = 0;

            foreach (DataRow row in dataTable.Rows)
            {
                models.Add(Activator.CreateInstance(type) as IModel);

                foreach (var property in properties)
                {
                    if (row[property.Name] == DBNull.Value)
                    {
                        continue;
                    }

                    var value = row[property.Name.ToLower()];

                    type.GetProperty(property.Name).SetValue(models[i], value);
                }

                i++;
            }

            await CloseConnection();

            return models.Cast<TModel>().ToList();
        }

        /// <summary>
        /// Конвертатция тип на mysqldbtype
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private MySqlDbType ConvertTypeToMysql(Type type)
        {
            if (type == typeof(short)) { return MySqlDbType.Int32; }
            else if (type == typeof(int)) { return MySqlDbType.Int32; }
            else if (type == typeof(long)) { return MySqlDbType.Int64; }
            else if (type == typeof(double)) { return MySqlDbType.Double; }
            else if (type == typeof(string)) { return MySqlDbType.VarChar; }
            else if (type == typeof(DateTime)) { return MySqlDbType.DateTime; }
            else if (type == typeof(bool)) { return MySqlDbType.Bit; }

            return MySqlDbType.VarChar;
        }
    }
}
