using System.Collections.Generic;
using SharedFunctionLib.Models;

namespace SharedFunctionLib.Business;

public static class NanoProgramMetaBusiness
{
    public static void AddOrUpdate(SimpleNanoProgramMetaModel model)
    {
        DAO.NanoProgramMetaDAO.AddOrUpdate(model);
    }
    
    public static void AddOrUpdate(string id, bool isEnabled, bool isWbVisible, int order)
    {
        var model = new SimpleNanoProgramMetaModel
        {
            ID = id,
            IsEnabled = isEnabled ? 1 : 0,
            IsWbVisible = isWbVisible ? 1 : 0,
            Order = order
        };
        DAO.NanoProgramMetaDAO.AddOrUpdate(model);
    }
    
    public static void Delete(string id)
    {
        DAO.NanoProgramMetaDAO.Delete(id);
    }
    
    public static void DeleteUnused(List<string> usedIds)
    {
        DAO.NanoProgramMetaDAO.DeleteUnused(usedIds);
    }

    public static List<SimpleNanoProgramMetaModel> GetAll()
    {
        return DAO.NanoProgramMetaDAO.GetAll();
    }
    
    public static SimpleNanoProgramMetaModel? GetById(string id)
    {
        return DAO.NanoProgramMetaDAO.Get(id);
    }
}