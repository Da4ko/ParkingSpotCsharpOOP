using System;
using System.Collections.Generic;

public class ParkingSpot
{
    private int id;
    private bool occupied;
    private string type;
    private double price;
    protected List<ParkingInterval> parkingIntervals;

    public int Id
    {
        get { return id; }
    }

    public bool Occupied
    {
        get { return occupied; }
        set { occupied = value; }
    }

    public string Type
    {
        get { return type; }
    }

    public double Price
    {
        get { return price; }
        set
        {
            if (value <= 0)
            {
                throw new ArgumentException("Parking price cannot be less or equal to 0!");
            }
            price = value;
        }
    }

    public ParkingSpot(int id, bool occupied, string type, double price)
    {
        this.id = id;
        this.occupied = occupied;
        this.type = type;
        Price = price; // Using the property to validate the price
        parkingIntervals = new List<ParkingInterval>();
    }

    public override string ToString()
    {
        return $"Parking Spot #{Id}\nOccupied: {Occupied}\nType: {Type}\nPrice per hour: {Price:F2} BGN";
    }

    public virtual bool ParkVehicle(string registrationPlate, int hoursParked, string type)
    {
        // Check if the spot is free and matches the vehicle type
        if (!Occupied && Type.Equals(type, StringComparison.OrdinalIgnoreCase))
        {
            ParkingInterval interval = new ParkingInterval(this, registrationPlate, hoursParked);
            parkingIntervals.Add(interval);
            return true;
        }
        return false;
    }

    public List<ParkingInterval> GetAllParkingIntervalsByRegistrationPlate(string registrationPlate)
    {
        List<ParkingInterval> result = new List<ParkingInterval>();
        foreach (var interval in parkingIntervals)
        {
            if (interval.RegistrationPlate.Equals(registrationPlate, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(interval);
            }
        }
        return result;
    }

    public double CalculateTotal()
    {
        double totalRevenue = 0;
        foreach (var interval in parkingIntervals)
        {
            totalRevenue += interval.Revenue;
        }
        return totalRevenue;
    }
}
public class CarParkingSpot : ParkingSpot
{
    public CarParkingSpot(int id, bool occupied, double price)
        : base(id, occupied, "car", price)
    {
    }
}

public class BusParkingSpot : ParkingSpot
{
    public BusParkingSpot(int id, bool occupied, double price)
        : base(id, occupied, "bus", price)
    {
    }
}

public class SubscriptionParkingSpot : ParkingSpot
{
    private string registrationPlate;

    public string RegistrationPlate
    {
        get { return registrationPlate; }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Registration plate can’t be null or empty!");
            }
            registrationPlate = value;
        }
    }

    public SubscriptionParkingSpot(int id, bool occupied, double price, string registrationPlate)
        : base(id, occupied, "subscription", price)
    {
        RegistrationPlate = registrationPlate; // Using the property to validate registration plate
    }

    public override bool ParkVehicle(string registrationPlate, int hoursParked, string type)
    {
        // Only allow parking if the registration plate matches the subscription
        if (!Occupied && RegistrationPlate.Equals(registrationPlate, StringComparison.OrdinalIgnoreCase))
        {
            ParkingInterval interval = new ParkingInterval(this, registrationPlate, hoursParked);
            parkingIntervals.Add(interval);
            return true;
        }
        return false;
    }
}
public class ParkingInterval
{
    private ParkingSpot parkingSpot;
    private string registrationPlate;
    private int hoursParked;

    public ParkingSpot ParklingSpot
    {
        get { return parkingSpot; }
    }

    public string RegistrationPlate
    {
        get { return registrationPlate; }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Registration plate can’t be null or empty!");
            }
            registrationPlate = value;
        }
    }

    public int HoursParked
    {
        get { return hoursParked; }
        set
        {
            if (value <= 0)
            {
                throw new ArgumentException("Hours parked can’t be zero or negative!");
            }
            hoursParked = value;
        }
    }

    public double Revenue
    {
        get
        {
            if (parkingSpot is SubscriptionParkingSpot)
            {
                return 0; // For subscription spots, revenue is always 0
            }
            return parkingSpot.Price * hoursParked;
        }
    }

    public ParkingInterval(ParkingSpot parkingSpot, string registrationPlate, int hoursParked)
    {
        this.parkingSpot = parkingSpot;
        RegistrationPlate = registrationPlate; // Using the property to validate registration plate
        HoursParked = hoursParked; // Using the property to validate hours parked
    }

    public override string ToString()
    {
        return $"Parking Spot #{ParklingSpot.Id}\nRegistrationPlate: {RegistrationPlate}\nHoursParked: {HoursParked}\nRevenue: {Revenue:F2} BGN";
    }
}
public class ParkingController
{
    private List<ParkingSpot> parkingSpots;

    public ParkingController()
    {
        parkingSpots = new List<ParkingSpot>();
    }

    public string CreateParkingSpot(List<string> args)
    {
        try
        {
            int id = int.Parse(args[0]);
            bool occupied = bool.Parse(args[1]);
            string type = args[2];
            double price = double.Parse(args[3]);

            if (type.ToLower() == "subscription")
            {
                string registrationPlate = args[4];
                var existingSpot = parkingSpots.FirstOrDefault(s => s.Id == id);
                if (existingSpot != null)
                {
                    return $"Parking spot {id} is already registered!";
                }

                SubscriptionParkingSpot spot = new SubscriptionParkingSpot(id, occupied, price, registrationPlate);
                parkingSpots.Add(spot);

                return $"Parking spot {spot.Id} was successfully registered in the system!";
            }
            else
            {
                var existingSpot = parkingSpots.FirstOrDefault(s => s.Id == id);
                if (existingSpot != null)
                {
                    return $"Parking spot {id} is already registered!";
                }

                ParkingSpot spot;
                if (type.ToLower() == "car")
                {
                    spot = new CarParkingSpot(id, occupied, price);
                }
                else if (type.ToLower() == "bus")
                {
                    spot = new BusParkingSpot(id, occupied, price);
                }
                else
                {
                    return "Unable to create parking spot!";
                }

                parkingSpots.Add(spot);
                return $"Parking spot {spot.Id} was successfully registered in the system!";
            }
        }
        catch (Exception)
        {
            return "Unable to create parking spot!";
        }
    }

    public string ParkVehicle(List<string> args)
    {
        try
        {
            int parkingSpotId = int.Parse(args[0]);
            string registrationPlate = args[1];
            int hoursParked = int.Parse(args[2]);
            string type = args[3];

            var parkingSpot = parkingSpots.FirstOrDefault(s => s.Id == parkingSpotId);

            if (parkingSpot == null)
            {
                return $"Parking spot {parkingSpotId} not found!";
            }

            if (parkingSpot.ParkVehicle(registrationPlate, hoursParked, type))
            {
                parkingSpot.Occupied = true;
                return $"Vehicle {registrationPlate} parked at {parkingSpotId} for {hoursParked} hours.";
            }
            else
            {
                return $"Vehicle {registrationPlate} can't park at {parkingSpotId}.";
            }
        }
        catch (Exception)
        {
            return "Unable to park vehicle!";
        }
    }

    public string FreeParkingSpot(List<string> args)
    {
        try
        {
            int parkingSpotId = int.Parse(args[0]);

            var parkingSpot = parkingSpots.FirstOrDefault(s => s.Id == parkingSpotId);

            if (parkingSpot == null)
            {
                return $"Parking spot {parkingSpotId} not found!";
            }

            if (parkingSpot.Occupied)
            {
                parkingSpot.Occupied = false;
                return $"Parking spot {parkingSpotId} is now free!";
            }
            else
            {
                return $"Parking spot {parkingSpotId} is not occupied.";
            }
        }
        catch (Exception)
        {
            return "Unable to free parking spot!";
        }
    }

    public string GetParkingSpotById(List<string> args)
    {
        try
        {
            int parkingSpotId = int.Parse(args[0]);

            var parkingSpot = parkingSpots.FirstOrDefault(s => s.Id == parkingSpotId);

            if (parkingSpot == null)
            {
                return $"Parking spot {parkingSpotId} not found!";
            }

            return parkingSpot.ToString();
        }
        catch (Exception)
        {
            return "Unable to get parking spot by id!";
        }
    }

    public string GetParkingIntervalsByParkingSpotIdAndRegistrationPlate(List<string> args)
    {
        try
        {
            int parkingSpotId = int.Parse(args[0]);
            string registrationPlate = args[1];

            var parkingSpot = parkingSpots.FirstOrDefault(s => s.Id == parkingSpotId);

            if (parkingSpot == null)
            {
                return $"Parking spot {parkingSpotId} not found!";
            }

            var intervals = parkingSpot.GetAllParkingIntervalsByRegistrationPlate(registrationPlate);

            if (intervals.Count == 0)
            {
                return $"No parking intervals found for vehicle with registration plate {registrationPlate} on parking spot {parkingSpotId}.";
            }

            return string.Join(Environment.NewLine, intervals.Select(i => i.ToString()));
        }
        catch (Exception)
        {
            return "Unable to get parking intervals by parking spot id and registration plate!";
        }
    }

    public string CalculateTotal()
    {
        try
        {
            double total = parkingSpots.Sum(s => s.CalculateTotal());
            return $"Total revenue from the parking: {total:F2} BGN";
        }
        catch (Exception)
        {
            return "Unable to calculate total revenue!";
        }
    }
}
public class Program
{
    public static void Main()
    {
        ParkingController parkingController = new ParkingController();
        string input;

        while ((input = Console.ReadLine()) != "End")
        {
            List<string> commandArgs = input.Split(':').ToList();
            string command = commandArgs[0];
            commandArgs.RemoveAt(0);

            string result = string.Empty;

            switch (command)
            {
                case "CreateParkingSpot":
                    result = parkingController.CreateParkingSpot(commandArgs);
                    break;
                case "ParkVehicle":
                    result = parkingController.ParkVehicle(commandArgs);
                    break;
                case "FreeParkingSpot":
                    result = parkingController.FreeParkingSpot(commandArgs);
                    break;
                case "GetParkingSpotById":
                    result = parkingController.GetParkingSpotById(commandArgs);
                    break;
                case "GetParkingIntervalsByParkingSpotIdAndRegistrationPlate":
                    result = parkingController.GetParkingIntervalsByParkingSpotIdAndRegistrationPlate(commandArgs);
                    break;
                case "CalculateTotal":
                    result = parkingController.CalculateTotal();
                    break;
            }

            Console.WriteLine(result);
        }
    }
}