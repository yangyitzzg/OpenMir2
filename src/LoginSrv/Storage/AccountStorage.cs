using LoginSrv.Conf;
using MySqlConnector;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using SystemModule.Extensions;
using SystemModule.Packets.ClientPackets;

namespace LoginSrv.Storage
{
    public class AccountStorage
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ConfigManager _configManager;
        private readonly Dictionary<string, AccountQuick> _accountMap;

        public AccountStorage(ConfigManager configManager)
        {
            _configManager = configManager;
            _accountMap = new Dictionary<string, AccountQuick>(StringComparer.OrdinalIgnoreCase);
        }

        private Config Config => _configManager.Config;

        public void Initialization()
        {
            _logger.Info("正在连接SQL服务器...");
            var dbConnection = new MySqlConnection(Config.ConnctionString);
            try
            {
                dbConnection.Open();
                _logger.Info("连接SQL服务器成功...");
                LoadQuickList();
            }
            catch (Exception ex)
            {
                _logger.Error("[错误] SQL 连接失败!请检查SQL设置...");
                _logger.Error(ex);
            }
            finally
            {
                dbConnection.Close();
                dbConnection.Dispose();
            }
        }

        private bool Open(ref MySqlConnection dbConnection)
        {
            bool result = false;
            if (dbConnection == null)
            {
                dbConnection = new MySqlConnection(Config.ConnctionString);
            }
            switch (dbConnection.State)
            {
                case ConnectionState.Open:
                    return true;
                case ConnectionState.Closed:
                    try
                    {
                        dbConnection.Open();
                        result = true;
                    }
                    catch (Exception e)
                    {
                        _logger.Error("打开数据库[MySql]失败.");
                        _logger.Error(e);
                        result = false;
                    }
                    break;
            }
            return result;
        }

        private void Close(MySqlConnection dbConnection)
        {
            if (dbConnection != null)
            {
                dbConnection.Close();
                dbConnection.Dispose();
            }
        }

        private void LoadQuickList()
        {
            const string sSQL = "SELECT Id,Account,State FROM account order by Id Desc";
            _accountMap.Clear();
            MySqlConnection dbConnection = null;
            if (!Open(ref dbConnection))
            {
                return;
            }
            try
            {
                var command = new MySqlCommand();
                command.CommandText = sSQL;
                command.Connection = dbConnection;
                using var dr = command.ExecuteReader();
                while (dr.Read())
                {
                    var nIndex = dr.GetInt32("Id");
                    var sAccount = dr.GetString("Account");
                    var boDeleted = dr.GetByte("State");
                    if (boDeleted == 0 && (!string.IsNullOrEmpty(sAccount)))
                    {
                        _accountMap.Add(sAccount, new AccountQuick(sAccount, nIndex));
                    }
                }
                dr.Close();
                dr.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error("读取账号列表失败.");
                _logger.Error(ex);
            }
            finally
            {
                Close(dbConnection);
            }
            _logger.Info($"账号数据读取成功.[{_accountMap.Count}]");
        }

        public int FindByName(string sName, ref IList<AccountQuick> List)
        {
            if (_accountMap.TryGetValue(sName, out var accountQuick))
            {
                List.Add(new AccountQuick(accountQuick.Account, accountQuick.Index));
            }
            return List.Count;
        }

        private bool GetAccount(int nIndex, ref AccountRecord accountRecord)
        {
            const string sSQL = "SELECT a.*,b.* FROM account a join account_protection b on a.Id=b.AccountId WHERE ID={0}";
            MySqlConnection dbConnection = null;
            if (!Open(ref dbConnection))
            {
                return false;
            }
            var command = new MySqlCommand();
            command.CommandText = string.Format(sSQL, nIndex);
            command.Connection = dbConnection;
            try
            {
                IDataReader dr = command.ExecuteReader();
                if (dr.Read())
                {
                    if (accountRecord == null)
                    {
                        accountRecord = new AccountRecord();
                        accountRecord.UserEntry = new UserEntry();
                        accountRecord.UserEntryAdd = new UserEntryAdd();
                    }
                    accountRecord.AccountId = dr.GetInt32("Id");
                    accountRecord.ErrorCount = dr.GetInt32("PassFailCount");
                    accountRecord.ActionTick = dr.GetInt32("PassFailTime");
                    accountRecord.PlayTime = dr.GetInt64("Seconds");
                    accountRecord.PayModel = dr.GetInt32("PayMode");
                    accountRecord.UserEntry.Account = dr.GetString("Account");
                    accountRecord.UserEntry.Password = dr.GetString("PassWord");
                    accountRecord.UserEntry.UserName = dr.GetString("UserName");
                    accountRecord.UserEntryAdd.Quiz2 = dr.GetString("Quiz2");
                }
                dr.Close();
                dr.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error("获取账号信息失败");
                _logger.Error(ex);
                return false;
            }
            finally
            {
                Close(dbConnection);
            }
            return true;
        }

        public int Index(string account)
        {
            if (_accountMap.TryGetValue(account, out var accountQuick))
            {
                return accountQuick.Index;
            }
            return -1;
        }

        public int Get(int nIndex, ref AccountRecord accountRecord)
        {
            int result = -1;
            if (nIndex < 0)
            {
                return result;
            }
            if (GetAccount(nIndex, ref accountRecord))
            {
                result = nIndex;
            }
            return result;
        }

        public int GetAccountPlayTime(string account)
        {
            var strSql = "SELECT Seconds FROM ACCOUNT WHERE Account=@Account";
            _logger.Debug("[SQL QUERY] " + strSql);
            MySqlConnection dbConnection = null;
            if (!Open(ref dbConnection))
            {
                return 0;
            }
            int result = 0;
            try
            {
                var command = new MySqlCommand();
                command.Connection = dbConnection;
                command.CommandText = strSql;
                command.Parameters.AddWithValue("@Account", account);
                var obj = command.ExecuteScalar();
                if (obj != null)
                {
                    result = (int)obj;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"获取账号[{account}]游戏时间失败");
                _logger.Error(e);
            }
            finally
            {
                Close(dbConnection);
            }
            return result;
        }

        public void UpdateAccountPlayTime(string account, long gameTime)
        {
            var strSql = "UPDATE ACCOUNT SET Seconds=@Seconds WHERE Account=@Account";
            _logger.Debug("[SQL QUERY] " + strSql);
            MySqlConnection dbConnection = null;
            if (!Open(ref dbConnection))
            {
                return;
            }
            try
            {
                var command = new MySqlCommand();
                command.Connection = dbConnection;
                command.CommandText = strSql;
                command.Parameters.AddWithValue("@Seconds", gameTime);
                command.Parameters.AddWithValue("@Account", account);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                _logger.Error($"更新账号[{account}]游戏时间失败");
                _logger.Error(e);
            }
            finally
            {
                Close(dbConnection);
            }
        }

        private int CreateAccount(AccountRecord accountRecord)
        {
            var result = 0;
            const string sqlStr = "INSERT INTO account (Account, PassWord, PassFailCount, PassFailTime, ValidFrom, ValidUntil, Seconds, StopUntil, PayMode, State, CreateTime, ModifyTime, LastLoginTime) VALUES (@Account, @PassWord, @PassFailCount, @PassFailTime,@ValidFrom, @ValidUntil, @Seconds,@StopUntil, @PayMode, @State, @CreateTime, @ModifyTime, @LastLoginTime);";
            MySqlConnection dbConnection = null;
            if (!Open(ref dbConnection))
            {
                return 0;
            }
            var beginTransaction = dbConnection.BeginTransaction();
            try
            {
                var command = new MySqlCommand();
                command.Transaction = beginTransaction;
                command.CommandText = sqlStr;
                command.Connection = dbConnection;
                command.Parameters.AddWithValue("@Account", accountRecord.UserEntry.Account);
                command.Parameters.AddWithValue("@PassWord", accountRecord.UserEntry.Password);
                command.Parameters.AddWithValue("@PassFailCount", 0);
                command.Parameters.AddWithValue("@PassFailTime", 0);
                command.Parameters.AddWithValue("@ValidFrom", DateTimeOffset.Now);
                command.Parameters.AddWithValue("@ValidUntil", DateTimeOffset.Now);
                command.Parameters.AddWithValue("@Seconds", 0);
                command.Parameters.AddWithValue("@StopUntil", DateTimeOffset.Now);
                command.Parameters.AddWithValue("@PayMode", 0);
                command.Parameters.AddWithValue("@State", 0);
                command.Parameters.AddWithValue("@CreateTime", DateTimeOffset.Now.ToUnixTimeSeconds());
                command.Parameters.AddWithValue("@ModifyTime", DateTimeOffset.Now.ToUnixTimeSeconds());
                command.Parameters.AddWithValue("@LastLoginTime", 0);
                command.ExecuteNonQuery();
                result = (int)command.LastInsertedId;

                command.CommandText = "INSERT INTO account_protection (AccountId, UserName, IdCard, Birthday, Phone, MobilePhone, ADDRESS1, ADDRESS2, EMail, Quiz1, Answer1, Quiz2, Answer2) VALUES(@AccountId, @UserName, @IdCard, @Birthday, @Phone, @MobilePhone, @ADDRESS1, @ADDRESS2, @EMail, @Quiz1, @Answer1, @Quiz2, @Answer2);";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@AccountId", result);
                command.Parameters.AddWithValue("@UserName", accountRecord.UserEntry.UserName);
                command.Parameters.AddWithValue("@IdCard", accountRecord.UserEntry.SSNo);
                command.Parameters.AddWithValue("@Birthday", accountRecord.UserEntryAdd.BirthDay);
                command.Parameters.AddWithValue("@Phone", accountRecord.UserEntryAdd.BirthDay);
                command.Parameters.AddWithValue("@MobilePhone", accountRecord.UserEntryAdd.BirthDay);
                command.Parameters.AddWithValue("@ADDRESS1", "");
                command.Parameters.AddWithValue("@ADDRESS2", "");
                command.Parameters.AddWithValue("@EMail", accountRecord.UserEntry.EMail);
                command.Parameters.AddWithValue("@Quiz1", accountRecord.UserEntry.Quiz);
                command.Parameters.AddWithValue("@Answer1", accountRecord.UserEntry.Answer);
                command.Parameters.AddWithValue("@Quiz2", accountRecord.UserEntryAdd.Quiz2);
                command.Parameters.AddWithValue("@Answer2", accountRecord.UserEntryAdd.Answer2);
                command.ExecuteNonQuery();
                beginTransaction.Commit();
            }
            catch (Exception ex)
            {
                beginTransaction.Rollback();
                _logger.Error("创建账号失败." + ex.Message);
                _logger.Error(ex);
            }
            finally
            {
                Close(dbConnection);
            }
            return result;
        }

        private int DeleteAccount(string account)
        {
            const string sUpdateRecord2 = "UPDATE account SET State=1, ModifyTime={0} WHERE Account='{1}'";
            MySqlConnection dbConnection = null;
            if (!Open(ref dbConnection))
            {
                return 0;
            }
            var result = 0;
            var command = new MySqlCommand();
            command.Connection = dbConnection;
            command.CommandText = string.Format(sUpdateRecord2, DateTimeOffset.Now.ToUnixTimeSeconds(), account);
            try
            {
                command.ExecuteNonQuery();
                result = 1;
            }
            catch (Exception ex)
            {
                result = 0;
                _logger.Error("[Exception] UpdateRecord");
                _logger.Error(ex);
            }
            finally
            {
                Close(dbConnection);
            }
            return result;
        }

        public int ChanggePassword(int accountId, string newPassword)
        {
            MySqlConnection dbConnection = null;
            if (!Open(ref dbConnection))
            {
                return 0;
            }
            var result = 0;
            try
            {
                var command = new MySqlCommand();
                command.Connection = dbConnection;
                command.CommandText = "UPDATE account SET PassWord = @PassWord, PassFailCount = 0, PassFailTime = 0, ModifyTime = @ModifyTime WHERE Id = @Id;";
                command.Parameters.AddWithValue("@PassWord", newPassword);
                command.Parameters.AddWithValue("@ModifyTime", DateTimeOffset.Now.ToUnixTimeSeconds());
                command.Parameters.AddWithValue("@Id", accountId);
                command.ExecuteNonQuery();
                result = 1;
            }
            catch (Exception E)
            {
                _logger.Error("[Exception] ChanggePassword");
                _logger.Error(E);
                return result;
            }
            finally
            {
                Close(dbConnection);
            }
            return result;
        }

        public int UpdateLoginRecord(AccountRecord accountRecord)
        {
            var result = 0;
            const string strSql = "UPDATE account SET ModifyTime=@ModifyTime, PassFailCount=@PassFailCount, PassFailTime=@PassFailTime,LastLoginTime=@LastLoginTime WHERE Id=@Id";
            MySqlConnection dbConnection = null;
            if (!Open(ref dbConnection))
            {
                return 0;
            }
            try
            {
                var command = new MySqlCommand();
                command.Connection = dbConnection;
                command.CommandText = strSql;
                command.Parameters.AddWithValue("@ModifyTime", DateTimeOffset.Now.ToUnixTimeSeconds());
                command.Parameters.AddWithValue("@PassFailCount", accountRecord.ErrorCount);
                command.Parameters.AddWithValue("@PassFailTime", accountRecord.ActionTick);
                command.Parameters.AddWithValue("@LastLoginTime", DateTimeOffset.Now.ToUnixTimeSeconds());
                command.Parameters.AddWithValue("@Id", accountRecord.AccountId);
                command.ExecuteNonQuery();
                result = 1;
            }
            catch (Exception E)
            {
                _logger.Error("[Exception] UpdateRecord");
                _logger.Error(E);
                return result;
            }
            finally
            {
                Close(dbConnection);
            }
            return result;
        }

        public int UpdateRecord(AccountRecord accountRecord)
        {
            var result = 0;
            const string strSql = "UPDATE account SET ModifyTime=@ModifyTime, PassFailCount=@PassFailCount, PassFailTime=@PassFailTime WHERE Id=@Id";
            MySqlConnection dbConnection = null;
            if (!Open(ref dbConnection))
            {
                return 0;
            }
            try
            {
                var command = new MySqlCommand();
                command.Connection = dbConnection;
                command.CommandText = strSql;
                command.Parameters.AddWithValue("@ModifyTime", DateTimeOffset.Now.ToUnixTimeSeconds());
                command.Parameters.AddWithValue("@PassFailCount", accountRecord.ErrorCount);
                command.Parameters.AddWithValue("@PassFailTime", accountRecord.ActionTick);
                command.Parameters.AddWithValue("@Id", accountRecord.AccountId);
                command.ExecuteNonQuery();
                result = 1;
            }
            catch (Exception E)
            {
                _logger.Error("[Exception] UpdateRecord");
                _logger.Error(E);
                return result;
            }
            finally
            {
                Close(dbConnection);
            }
            return result;
        }

        public bool Update(int nIndex, ref AccountRecord accountRecord)
        {
            bool result = false;
            if (nIndex < 0)
            {
                return false;
            }
            if (_accountMap.Count <= nIndex)
            {
                return false;
            }
            if (UpdateRecord(accountRecord) > 0)
            {
                result = true;
            }
            return result;
        }

        public int UpdateAccount(int nIndex, ref AccountRecord accountRecord)
        {
            var result = 0;
            MySqlConnection dbConnection = null;
            if (!Open(ref dbConnection))
            {
                return 0;
            }
            var beginTransaction = dbConnection.BeginTransaction();
            try
            {
                var command = new MySqlCommand();
                command.Transaction = beginTransaction;
                command.CommandText = "UPDATE account SET PassWord = @PassWord, PassFailCount = @PassFailCount, PassFailTime = @PassFailTime, ModifyTime = @ModifyTime, LastLoginTime = @LastLoginTime WHERE Id = @Id;";
                command.Connection = dbConnection;
                command.Parameters.AddWithValue("@Id", nIndex);
                command.Parameters.AddWithValue("@PassWord", accountRecord.UserEntry.Password);
                command.Parameters.AddWithValue("@PassFailCount", 0);
                command.Parameters.AddWithValue("@PassFailTime", 0);
                command.Parameters.AddWithValue("@ModifyTime", DateTimeOffset.Now.ToUnixTimeSeconds());
                command.Parameters.AddWithValue("@LastLoginTime", 0);
                command.ExecuteNonQuery();

                command.CommandText = "UPDATE account_protection SET UserName = @UserName, IdCard = @IdCard, Birthday = @Birthday, Phone = @Phone, MobilePhone = @MobilePhone, ADDRESS1 = @ADDRESS1, ADDRESS2 = @ADDRESS2, EMail = @EMail, Quiz1 = @Quiz1, Answer1 = @Answer1, Quiz2 = @Quiz2, Answer2 = @Answer2 WHERE AccountId = @AccountId;";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@AccountId", nIndex);
                command.Parameters.AddWithValue("@UserName", accountRecord.UserEntry.UserName);
                command.Parameters.AddWithValue("@IdCard", accountRecord.UserEntry.SSNo);
                command.Parameters.AddWithValue("@Birthday", accountRecord.UserEntryAdd.BirthDay);
                command.Parameters.AddWithValue("@Phone", accountRecord.UserEntryAdd.MobilePhone);
                command.Parameters.AddWithValue("@MobilePhone", accountRecord.UserEntryAdd.MobilePhone);
                command.Parameters.AddWithValue("@ADDRESS1", "");
                command.Parameters.AddWithValue("@ADDRESS2", "");
                command.Parameters.AddWithValue("@EMail", accountRecord.UserEntry.EMail);
                command.Parameters.AddWithValue("@Quiz1", accountRecord.UserEntry.Quiz);
                command.Parameters.AddWithValue("@Answer1", accountRecord.UserEntry.Answer);
                command.Parameters.AddWithValue("@Quiz2", accountRecord.UserEntryAdd.Quiz2);
                command.Parameters.AddWithValue("@Answer2", accountRecord.UserEntryAdd.Answer2);
                command.ExecuteNonQuery();
                beginTransaction.Commit();
                result = 1;
            }
            catch (Exception ex)
            {
                beginTransaction.Rollback();
                _logger.Error("创建账号失败." + ex.Message);
                _logger.Error(ex);
            }
            finally
            {
                Close(dbConnection);
            }
            return result;
        }

        public bool Add(AccountRecord accountRecord)
        {
            bool result;
            var sAccount = accountRecord.UserEntry.Account;
            if (Index(sAccount) > 0)
            {
                result = false;
            }
            else
            {
                var nIndex = CreateAccount(accountRecord);
                if (nIndex > 0)
                {
                    _accountMap.Add(sAccount, new AccountQuick(sAccount, nIndex));
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }

        public bool Delete(int nIndex, ref AccountRecord accountRecord)
        {
            var result = false;
            if (nIndex < 0)
            {
                return false;
            }
            if (_accountMap.Count <= nIndex)
            {
                return false;
            }
            var up = DeleteAccount(accountRecord.UserEntry.Account);
            if (up > 0)
            {
                _accountMap.Remove(accountRecord.UserEntry.Account);
                result = true;
            }
            return result;
        }
    }
}