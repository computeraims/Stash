﻿using MySql.Data.MySqlClient;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stash.Database
{
    public class StashDB
    {
        static string Address = Main.Config.address;
        static string Name = Main.Config.name;
        static string Table = Main.Config.table;
        static string Username = Main.Config.username;
        static string Password = Main.Config.password;
        static int Port = Main.Config.port;

        internal StashDB()
        {
            new I18N.West.CP1250();
            MySqlConnection connection = CreateConnection();
            try
            {
                connection.Open();
                connection.Close();

                CreateCheckSchema();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex);
                //Main.Instance.UnloadPlugin();
            }
        }

        private static MySqlConnection CreateConnection()
        {
            MySqlConnection connection = null;
            try
            {
                connection = new MySqlConnection($"SERVER={Address};DATABASE={Name};UID={Username};PASSWORD={Password};PORT={Port};");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return connection;
        }

        private void CreateCheckSchema()
        {
            using (MySqlConnection connection = CreateConnection())
            {
                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    connection.Open();
                    command.CommandText = "SHOW TABLES LIKE '" + Table + "';";

                    object test = command.ExecuteScalar();
                    if (test == null)
                    {
                        Console.WriteLine($"{Table} table not found, creating!");
                        command.CommandText =
                            $@"CREATE TABLE `{Table}`
                            (
                                `steamID` VARCHAR(32) DEFAULT NULL,
                                `itemID` INT(4) DEFAULT NULL,
                                `x` INT(11) DEFAULT NULL,
                                `y` INT(11) DEFAULT NULL,
                                `rotation` INT(11) DEFAULT NULL,
                                `durability` INT(3) DEFAULT NULL,
                                `amount` INT(11) DEFAULT NULL,
                                `metadata` VARCHAR(255) DEFAULT NULL
                            ) COLLATE = 'utf8_general_ci' ENGINE = InnoDB;";
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private static async Task<List<ItemJar>> GetItemsAsync(string steamID)
        {
            using (MySqlConnection connection = CreateConnection())
            {
                List<ItemJar> items = new List<ItemJar>();

                try
                {
                    MySqlCommand command = new MySqlCommand
                    (
                        $@"SELECT * FROM {Table}
                        WHERE steamID = @SteamID", connection
                    );

                    command.Parameters.AddWithValue("@SteamID", steamID);
                    await connection.OpenAsync();
                    var dataReader = await command.ExecuteReaderAsync();

                    while (dataReader.Read())
                    {
                        Console.WriteLine($"Data Reader Found Item: {dataReader["itemID"]}");

                        items.Add(new ItemJar((byte)Convert.ToInt32(dataReader["x"]), (byte)Convert.ToInt32(dataReader["y"]), (byte)Convert.ToInt32(dataReader["rotation"]), new Item(Convert.ToUInt16(dataReader["itemID"]), true)
                        {
                            durability = (byte)Convert.ToInt32(dataReader["durability"]),
                            metadata = Encoding.UTF8.GetBytes(dataReader["metadata"].ToString()),
                            amount = (byte)Convert.ToInt32(dataReader["amount"])
                        }));
                    }
                    dataReader.Close();
                    await connection.CloseAsync();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                return items;
            }
        }

        public static List<ItemJar> GetItems(string steamID)
        {
            return GetItemsAsync(steamID).GetAwaiter().GetResult();
        }

        private static async Task AddItemAsync(string steamID, ItemJar item)
        {
            string metatext = (item.item.metadata is null) ? "" : Encoding.UTF8.GetString(item.item.metadata);

            using (MySqlConnection connection = CreateConnection())
            {
                try
                {
                    MySqlCommand command = new MySqlCommand
                    (
                        $@"INSERT INTO {Table} (steamID, itemID, x, y, rotation, durability, amount, metadata)
                        VALUES (@SteamID, @ItemID, @X, @Y, @Rotation, @Durability, @Amount, @Metadata);", connection
                    );

                    command.Parameters.AddWithValue("@SteamID", steamID);
                    command.Parameters.AddWithValue("@ItemID", item.item.id);
                    command.Parameters.AddWithValue("@X", item.x);
                    command.Parameters.AddWithValue("@Y", item.y);
                    command.Parameters.AddWithValue("@Rotation", item.rot);
                    command.Parameters.AddWithValue("@Durability", item.item.durability);
                    command.Parameters.AddWithValue("@Amount", item.item.amount);
                    command.Parameters.AddWithValue("@Metadata", metatext);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public static void AddItem(string steamID, ItemJar item)
        {
            AddItemAsync(steamID, item).GetAwaiter().GetResult();
        }

        private static async Task DeleteItemAsync(string steamID, ItemJar item)
        {
            string metatext = (item.item.metadata is null) ? "" : Encoding.UTF8.GetString(item.item.metadata);

            using (MySqlConnection connection = CreateConnection())
            {
                try
                {
                    MySqlCommand command = new MySqlCommand
                    (
                        $@"DELETE FROM {Table} WHERE `steamID` = @SteamID AND `itemID` = @ItemID AND `durability` = @Durability AND `amount` = @Amount and `metadata` = @Metadata LIMIT 1;", connection
                    );

                    command.Parameters.AddWithValue("@SteamID", steamID);
                    command.Parameters.AddWithValue("@ItemID", item.item.id);
                    command.Parameters.AddWithValue("@Durability", item.item.durability);
                    command.Parameters.AddWithValue("@Amount", item.item.amount);
                    command.Parameters.AddWithValue("@Metadata", metatext);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public static void DeleteItem(string steamID, ItemJar item)
        {
            DeleteItemAsync(steamID, item).GetAwaiter().GetResult();
        }

        private static async Task UpdateItemAsync(string steamID, ItemJar item)
        {
            string metatext = (item.item.metadata is null) ? "" : Encoding.UTF8.GetString(item.item.metadata);

            using (MySqlConnection connection = CreateConnection())
            {
                try
                {
                    MySqlCommand command = new MySqlCommand
                    (
                        $@"UPDATE {Table}
                        SET `metadata` = @Metadata WHERE `steamID` = @SteamID AND `itemID` = @ItemID LIMIT 1", connection
                    );

                    command.Parameters.AddWithValue("@SteamID", steamID);
                    command.Parameters.AddWithValue("@Metadata", metatext);
                    command.Parameters.AddWithValue("@ItemID", item.item.id);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public static void UpdateItem(string steamID, ItemJar item)
        {
            UpdateItemAsync(steamID, item).GetAwaiter().GetResult();
        }
    }
}