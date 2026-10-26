using Kinevor.Modules.Trainers.Domain;
using Kinevor.Modules.Trainers.EntityFrameworkCore;
using Kinevor.Platform.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kinevor.Modules.Trainers.DemoApi.Data;

/// <summary>Seeds a realistic trainer directory for the standalone demo.</summary>
public static class DemoDataSeeder
{
    /// <summary>Adds demo trainers when the database is empty.</summary>
    public static async Task SeedAsync(TrainersDbContext db, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(db);

        if (await db.Trainers.AnyAsync(ct))
            return;

        var trainers = new[]
        {
            Build("Layla", "Hassan", "layla.hassan@kinevor.demo", "+201001234567",
                "Yoga and mobility coach focused on safe progression for every level.",
                [ClassType.Yoga, ClassType.Pilates, ClassType.Stretching],
                ["RYT-500", "Pilates Level 2"], 8, 4.8, 126),
            Build("Omar", "Khaled", "omar.khaled@kinevor.demo", "+201002345678",
                "Conditioning specialist who blends HIIT intervals with functional strength.",
                [ClassType.HIIT, ClassType.CrossFit, ClassType.Boxing],
                ["NSCA-CSCS", "CrossFit Level 2"], 6, 4.7, 98),
            Build("Nour", "Adel", "nour.adel@kinevor.demo", "+201003456789",
                "High-energy cycling and dance coach with a focus on rhythm and endurance.",
                [ClassType.Spinning, ClassType.Zumba, ClassType.Aqua],
                ["Spinning Instructor", "Zumba Licensed"], 5, 4.9, 143),
            Build("Karim", "Fathy", "karim.fathy@kinevor.demo", "+201004567890",
                "Combat sports coach with a decade of ring and conditioning experience.",
                [ClassType.Boxing, ClassType.Kickboxing, ClassType.HIIT],
                ["USA Boxing Coach", "First Aid"], 10, 4.6, 87),
            Build("Salma", "Youssef", "salma.youssef@kinevor.demo", "+201005678901",
                "Mind-body coach combining mobility, breathwork and guided meditation.",
                [ClassType.Stretching, ClassType.Meditation, ClassType.Yoga],
                ["RYT-200", "Meditation Facilitator"], 7, 4.9, 112),
            Build("Youssef", "Ibrahim", "youssef.ibrahim@kinevor.demo", "+201006789012",
                "Strength coach leading barbell pump and low-impact aqua sessions.",
                [ClassType.BodyPump, ClassType.Aqua, ClassType.CrossFit],
                ["NASM-CPT", "Aqua Fitness"], 4, 4.5, 64),
        };

        db.Trainers.AddRange(trainers);
        await db.SaveChangesAsync(ct);
    }

    private static Trainer Build(
        string firstName, string lastName, string email, string phone, string bio,
        List<ClassType> specializations, List<string> certifications,
        int yearsExperience, double rating, int totalRatings)
    {
        var result = Trainer.Create(firstName, lastName, email, phone, bio,
            specializations, certifications, yearsExperience);
        if (result.IsFailure)
            throw new InvalidOperationException($"Seed trainer '{firstName} {lastName}' is invalid: {result.Error.Message}");

        var trainer = result.Value;
        for (var i = 0; i < totalRatings; i++)
            trainer.AddRating(rating);

        return trainer;
    }
}
