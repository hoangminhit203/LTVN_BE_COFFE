using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IFlavorNotesService
    {
        Task<ActionResult<ResponseResult>?> CreateFlavorNotes(FlavorNoteCreateVModel request);
        Task<ActionResult<ResponseResult>?> GetFlavorNotesById(int id);
        Task<ActionResult<ResponseResult>?> UpdateFlavorNotes(int id, FlavorNoteUpdateVModel request);
        Task<ActionResult<ResponseResult>?> DeleteFlavorNotes(int id);
        Task<ActionResult<ResponseResult>> GetAllFlavorNotes(FlavorNoteFilterVModel filter);
    }
}
