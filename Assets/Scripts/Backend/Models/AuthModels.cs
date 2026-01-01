using System;

[Serializable]
public class RequestOtpRequest
{
    public string email;
}

[Serializable]
public class RequestOtpResponse
{
    public bool success;
    public string message;
}

[Serializable]
public class VerifyOtpRequest
{
    public string email;
    public string otp;
}

[Serializable]
public class VerifyOtpResponse
{
    public bool success;
    public string token;
    public string message;
}

