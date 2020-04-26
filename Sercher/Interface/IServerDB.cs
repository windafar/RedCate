namespace Sercher
{
    public interface IServerDB
    {
        string DbName { get; set; }
        string Ip { get; set; }
        int Status { get; set; }

        bool CreateDB();
        void DeleDb();
        bool GetdbStatus();
    }
}