namespace Sercher
{
    public interface IPictureDB : IServerDB
    {
        Document GetDocumentByPic(int Picid);

    }
}