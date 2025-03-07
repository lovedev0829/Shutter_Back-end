using ShutterManagement.Core;
using ShutterManagement.Core.SPModel;
using ShutterManagement.GHL.Service;
public class GhlContactResponse
{
    public List<Customer> Contacts { get; set; }
}

public class ContactResponse
{
    public Customer Contact { get; set; }
}