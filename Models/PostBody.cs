namespace DotNet9WebApi.Models;

public class PollBody
{
	public string Option1 { set; get; }
	public string Option2 { set; get; }
}

public class PostBody
{
	public string Content { set; get; }
	public PollBody? Poll { set; get; }
	public IFormFile? Media { set; get; }
	public int? IntProp { set; get; }
	public int[]? IntArrProp { set; get; }
}