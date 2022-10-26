﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebApiFinbeCore.Models
{
    [DataContract]
    public class ApiResponse<T> where T : class
    {
        [DataMember(Name = "success")]
        public bool Success { get; set; }
        [DataMember(Name = "result")]
        public string Result { get; set; }
        [DataMember(Name = "code")]
        public int StatusCode { get; set; }
        [DataMember(Name = "data")]
        public T ResponseData { get; set; }
        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}