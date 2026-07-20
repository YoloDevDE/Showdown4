namespace Showdown4.Entities;

public class Result
{
	public Result(Racer racer, double time)
	{
		Racer = racer;
		Time = time;
	}

	public Racer Racer { get; set; }
	public double Time { get; set; }
}