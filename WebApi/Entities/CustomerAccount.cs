using System;
using System.Collections.Generic;

namespace WebApi.Entities;

public partial class CustomerAccount
{
    public int AccountId { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }
}
