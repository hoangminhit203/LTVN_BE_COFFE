using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

public interface IShippingAddressService
{
 
    Task<ActionResult<ResponseResult>?> CreateShippingAddress(ShippingAddressCreateVModel request);

    Task<ActionResult<ResponseResult>?> UpdateShippingAddress(ShippingAddressUpdateVModel request, int id);

    Task<ActionResult<ResponseResult>> DeleteShippingAddress(int id);

  
    Task<ActionResult<ResponseResult>?> GetShippingAddress(int id);

 
    Task<ActionResult<ResponseResult>> GetAllShippingAddresses(ShippingAddressFilterVModel filter);
}