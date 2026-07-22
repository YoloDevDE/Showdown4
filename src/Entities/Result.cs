namespace Showdown4.Entities;

public class Result(Racer racer, double time)
{
	public Racer Racer { get; set; } = racer;
	public double Time { get; set; } = time;
}