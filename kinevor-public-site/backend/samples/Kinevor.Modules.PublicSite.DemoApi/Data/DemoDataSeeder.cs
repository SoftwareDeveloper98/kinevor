using Kinevor.Modules.PublicSite.Domain;
using Kinevor.Modules.PublicSite.EntityFrameworkCore;

namespace Kinevor.Modules.PublicSite.DemoApi.Data;

/// <summary>Seeds deterministic public-site content for the standalone demo.</summary>
public static class DemoDataSeeder
{
    /// <summary>Identifier of the primary seeded branch.</summary>
    public static readonly Guid MainBranchId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    /// <summary>Identifier of the downtown branch.</summary>
    public static readonly Guid DowntownBranchId = Guid.Parse("11111111-1111-1111-1111-111111111112");

    /// <summary>Identifier of the westside branch.</summary>
    public static readonly Guid WestsideBranchId = Guid.Parse("11111111-1111-1111-1111-111111111113");

    /// <summary>Identifier of the featured membership plan.</summary>
    public static readonly Guid PerformancePlanId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    /// <summary>Identifier of the first coach.</summary>
    public static readonly Guid SarahTrainerId = Guid.Parse("33333333-3333-3333-3333-333333333331");

    /// <summary>Identifier of the first upcoming class.</summary>
    public static readonly Guid FirstClassId = Guid.Parse("44444444-4444-4444-4444-444444444441");

    /// <summary>Adds demo content once, using deterministic identifiers.</summary>
    public static async Task SeedAsync(
        PublicSiteDbContext db,
        TimeProvider timeProvider,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (db.SiteProfiles.Any())
            return;

        var now = timeProvider.GetLocalNow().DateTime;
        var profileId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        db.SiteProfiles.Add(SiteProfile.Seed(
            profileId,
            "Kinevor",
            "Built to move. Made to belong.",
            "Three neighbourhood gyms, one standard of coaching. Strength, conditioning and recovery under one roof.",
            "Kinevor opened its first training floor in Tribeca in 2016 with a simple idea: serious training should still feel welcoming. A decade later, three locations share the same coaches, the same equipment standards and the same belief that progress belongs to everyone. Members get a plan, a team and a room full of people moving in the same direction.",
            "New York's strength and wellness house",
            "+1 (212) 555-0123",
            "hello@kinevorgym.com",
            "123 Main Street",
            "New York, NY 10013",
            "@kinevorgym",
            2400,
            24,
            85,
            4.9m,
            2016,
            [
                "24/7 member access at Main",
                "Small-group coaching",
                "Recovery suite and sauna",
                "No long-term lock-in",
            ]));

        db.Branches.AddRange(
            Branch.Seed(
                MainBranchId,
                "Kinevor Main",
                "main",
                "Tribeca",
                "123 Main Street",
                "New York, NY 10013",
                "+1 (212) 555-0123",
                "Mon-Fri 05:30-23:00 | Sat-Sun 07:00-20:00",
                "Underground garage with two hours free for class bookings.",
                ["Strength floor", "Cardio zone", "Pool", "Sauna", "Group studio"],
                250,
                40.7128,
                -74.0060,
                "ember",
                1),
            Branch.Seed(
                DowntownBranchId,
                "Kinevor Downtown",
                "downtown",
                "Financial District",
                "45 Commerce Boulevard",
                "New York, NY 10005",
                "+1 (212) 555-0144",
                "Mon-Fri 05:30-23:00 | Sat-Sun 07:00-19:00",
                "Partner garage on 5th Street with evening validation.",
                ["Strength floor", "Cardio zone", "Climbing wall", "Recovery lounge", "Group studio"],
                180,
                40.7060,
                -74.0086,
                "ocean",
                2),
            Branch.Seed(
                WestsideBranchId,
                "Kinevor Westside",
                "westside",
                "Upper West Side",
                "900 Riverside Drive",
                "New York, NY 10025",
                "+1 (212) 555-0177",
                "Mon-Fri 06:00-21:00 | Sat-Sun 08:00-18:00",
                "Free on-site lot for members and trial guests.",
                ["Strength floor", "Pool", "Sauna", "Kids club", "Outdoor track"],
                150,
                40.8010,
                -73.9630,
                "forest",
                3));

        db.MembershipPlans.AddRange(
            MembershipPlan.Seed(
                Guid.Parse("22222222-2222-2222-2222-222222222221"),
                "Essential",
                "essential",
                "Everything you need for independent training, at any Kinevor location.",
                "Essential",
                39m,
                "USD",
                1,
                ["Gym floor and cardio access", "All three locations", "Locker room and towels", "Member app"],
                1),
            MembershipPlan.Seed(
                PerformancePlanId,
                "Performance",
                "performance",
                "Unlimited group classes plus quarterly coaching check-ins to keep progress honest.",
                "Performance",
                69m,
                "USD",
                1,
                ["Everything in Essential", "Unlimited group classes", "Quarterly coach check-in", "Recovery suite access"],
                2,
                isFeatured: true,
                badge: "Most popular"),
            MembershipPlan.Seed(
                Guid.Parse("22222222-2222-2222-2222-222222222223"),
                "Unlimited",
                "unlimited",
                "The full Kinevor experience with priority booking, guest passes and spa access.",
                "Unlimited",
                109m,
                "USD",
                1,
                ["Everything in Performance", "Priority class booking", "Spa and sauna access", "Two guest passes each month"],
                3,
                badge: "Best value"),
            MembershipPlan.Seed(
                Guid.Parse("22222222-2222-2222-2222-222222222224"),
                "Elite Annual",
                "elite-annual",
                "A year of coaching, recovery and accountability with personal training included.",
                "Elite",
                899m,
                "USD",
                12,
                ["Everything in Unlimited", "12 personal training sessions", "Nutrition consultation", "Four guest passes"],
                4),
            MembershipPlan.Seed(
                Guid.Parse("22222222-2222-2222-2222-222222222225"),
                "Day Pass",
                "day-pass",
                "A single-day visitor pass for the Main training floor.",
                "Visitor",
                25m,
                "USD",
                1,
                ["One-day gym access", "Locker and towel"],
                5,
                isPublished: false));

        db.Trainers.AddRange(
            Trainer.Seed(
                SarahTrainerId,
                "Sarah",
                "Mitchell",
                "sarah-mitchell",
                "Strength and conditioning lead",
                "Sarah builds athletes from the floor up. Her sessions blend barbell fundamentals, conditioning and enough humour to make the last round possible.",
                ["Strength", "HIIT", "CrossFit"],
                ["CrossFit L2", "NASM-CPT", "USA Weightlifting L1"],
                9,
                MainBranchId,
                "Kinevor Main",
                1),
            Trainer.Seed(
                Guid.Parse("33333333-3333-3333-3333-333333333332"),
                "Carlos",
                "Reyes",
                "carlos-reyes",
                "Endurance and cycling coach",
                "A former competitive cyclist, Carlos knows how to pace a room. Expect clear targets, smart intervals and a playlist that keeps the legs turning.",
                ["Spinning", "Endurance", "Conditioning"],
                ["Spinning Instructor Certification", "ACE-CPT"],
                7,
                MainBranchId,
                "Kinevor Main",
                2),
            Trainer.Seed(
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                "Yuki",
                "Tanaka",
                "yuki-tanaka",
                "Yoga and mobility coach",
                "Yuki teaches strength through range. Her flows combine breath, mobility and quiet attention, with options for every body in the room.",
                ["Yoga", "Mobility", "Meditation"],
                ["RYT-500", "Meditation Teacher Certification"],
                11,
                WestsideBranchId,
                "Kinevor Westside",
                3),
            Trainer.Seed(
                Guid.Parse("33333333-3333-3333-3333-333333333334"),
                "Amara",
                "Diallo",
                "amara-diallo",
                "Group fitness coach",
                "Amara brings the room together. Her dance conditioning and BodyPump classes are high-energy, inclusive and built around repeatable technique.",
                ["Zumba", "BodyPump", "Dance fitness"],
                ["Zumba Licensed Instructor", "Les Mills BodyPump"],
                6,
                DowntownBranchId,
                "Kinevor Downtown",
                4));

        var day = now.Date;
        var sarah = "Sarah Mitchell";
        var carlos = "Carlos Reyes";
        var yuki = "Yuki Tanaka";
        var amara = "Amara Diallo";
        var main = "Kinevor Main";
        var downtown = "Kinevor Downtown";
        var westside = "Kinevor Westside";

        db.ClassSessions.AddRange(
            ClassSession.Seed(
                FirstClassId,
                "Sunrise HIIT",
                "Forty-five focused minutes of intervals, sled pushes and loaded carries before the day starts.",
                "HIIT",
                "High",
                SarahTrainerId,
                sarah,
                day.AddDays(1).AddHours(7),
                45,
                20,
                "Studio A",
                MainBranchId,
                main),
            ClassSession.Seed(
                Guid.Parse("44444444-4444-4444-4444-444444444442"),
                "Vinyasa Flow",
                "A steady, breath-led flow that opens the hips and shoulders and finishes with a long reset.",
                "Yoga",
                "Moderate",
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                yuki,
                day.AddDays(1).AddHours(18).AddMinutes(30),
                60,
                22,
                "Studio B",
                WestsideBranchId,
                westside),
            ClassSession.Seed(
                Guid.Parse("44444444-4444-4444-4444-444444444443"),
                "Power Ride",
                "Three climbs, three sprints and one very good playlist. All levels welcome.",
                "Cycling",
                "High",
                Guid.Parse("33333333-3333-3333-3333-333333333332"),
                carlos,
                day.AddDays(2).AddHours(9),
                50,
                18,
                "Cycle Room",
                MainBranchId,
                main),
            ClassSession.Seed(
                Guid.Parse("44444444-4444-4444-4444-444444444444"),
                "Dance Cardio",
                "Big movement, simple choreography and zero judgement. Come for the sweat, stay for the energy.",
                "Dance",
                "Moderate",
                Guid.Parse("33333333-3333-3333-3333-333333333334"),
                amara,
                day.AddDays(2).AddHours(18),
                45,
                30,
                "Main Floor",
                DowntownBranchId,
                downtown),
            ClassSession.Seed(
                Guid.Parse("44444444-4444-4444-4444-444444444445"),
                "Barbell Basics",
                "Squat, press and pull technique in a small-group setting. Perfect for your first cycle.",
                "Strength",
                "Moderate",
                SarahTrainerId,
                sarah,
                day.AddDays(3).AddHours(7).AddMinutes(30),
                60,
                12,
                "Platform",
                MainBranchId,
                main),
            ClassSession.Seed(
                Guid.Parse("44444444-4444-4444-4444-444444444446"),
                "Evening Reset",
                "Guided breathwork, mobility and meditation to close the training day.",
                "Recovery",
                "Low",
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                yuki,
                day.AddDays(3).AddHours(19),
                45,
                16,
                "Wellness Room",
                WestsideBranchId,
                westside),
            ClassSession.Seed(
                Guid.Parse("44444444-4444-4444-4444-444444444447"),
                "Full Body Power",
                "A full-body strength circuit with compound lifts and a short conditioning finisher.",
                "Strength",
                "High",
                SarahTrainerId,
                sarah,
                day.AddDays(4).AddHours(10),
                55,
                18,
                "Studio A",
                MainBranchId,
                main),
            ClassSession.Seed(
                Guid.Parse("44444444-4444-4444-4444-444444444448"),
                "Saturday Sweat",
                "Teams, stations and a finisher you will talk about on Monday.",
                "Conditioning",
                "High",
                Guid.Parse("33333333-3333-3333-3333-333333333332"),
                carlos,
                day.AddDays(5).AddHours(9).AddMinutes(30),
                50,
                24,
                "Main Floor",
                MainBranchId,
                main),
            ClassSession.Seed(
                Guid.Parse("44444444-4444-4444-4444-444444444449"),
                "Draft Pilates",
                "A draft class used to verify publication filtering.",
                "Pilates",
                "Low",
                SarahTrainerId,
                sarah,
                day.AddDays(4).AddHours(12),
                45,
                15,
                "Studio B",
                MainBranchId,
                main,
                isPublished: false));

        db.FaqEntries.AddRange(
            FaqEntry.Seed(
                Guid.Parse("55555555-5555-5555-5555-555555555551"),
                "Membership",
                "Can I try Kinevor before joining?",
                "Yes. Book a free trial and choose a class or a training-floor visit. A coach will meet you at reception, show you around and help you settle in.",
                1),
            FaqEntry.Seed(
                Guid.Parse("55555555-5555-5555-5555-555555555552"),
                "Membership",
                "Can I freeze my membership?",
                "You can freeze for up to three months each year from the member app or at reception. Your rate and joining date stay exactly where they are.",
                2),
            FaqEntry.Seed(
                Guid.Parse("55555555-5555-5555-5555-555555555553"),
                "Classes",
                "Do I need to book classes in advance?",
                "We recommend it. Members can book seven days ahead, and Unlimited members get priority booking at fourteen days. Waitlists promote automatically.",
                3),
            FaqEntry.Seed(
                Guid.Parse("55555555-5555-5555-5555-555555555554"),
                "Coaching",
                "Are personal training sessions included?",
                "Performance includes a quarterly coach check-in. Personal training is available as an add-on to any plan, and Elite Annual includes twelve sessions.",
                4),
            FaqEntry.Seed(
                Guid.Parse("55555555-5555-5555-5555-555555555555"),
                "Facilities",
                "What is in the recovery suite?",
                "Each location has a sauna, cold plunge, compression boots and a quiet room. Main also has a pool. Access is included from Performance upward.",
                5),
            FaqEntry.Seed(
                Guid.Parse("55555555-5555-5555-5555-555555555556"),
                "Billing",
                "Is there a joining fee?",
                "No. There is no joining fee and no long-term lock-in on monthly plans. We ask for 30 days' notice if you decide to cancel.",
                6),
            FaqEntry.Seed(
                Guid.Parse("55555555-5555-5555-5555-555555555557"),
                "Billing",
                "Draft billing question",
                "This answer is intentionally hidden from the public site.",
                7,
                isPublished: false));

        db.BlogPosts.AddRange(
            BlogPost.Seed(
                Guid.Parse("66666666-6666-6666-6666-666666666661"),
                "strength-that-travels",
                "Strength that travels",
                "Why the best training plan is the one you can repeat on a busy Tuesday.",
                "<p>The most useful strength programme is not the most complicated one. It is the one you can repeat when work runs late, when sleep is short and when motivation is average.</p><p>At Kinevor we build around five patterns: squat, hinge, push, pull and carry. Master those, progress patiently and let consistency do the rest.</p>",
                "Sarah Mitchell",
                new DateTime(2026, 10, 8),
                imageUrl: null),
            BlogPost.Seed(
                Guid.Parse("66666666-6666-6666-6666-666666666662"),
                "a-better-first-week",
                "A better first week",
                "Four small choices that make your first seven days at Kinevor stick.",
                "<p>Start with two sessions, not five. Book a coach introduction. Leave one day completely free of training. Write down how you want to feel in eight weeks.</p><p>Momentum is built by repeatable wins, not heroic first weeks.</p>",
                "Amara Diallo",
                new DateTime(2026, 9, 24),
                imageUrl: null),
            BlogPost.Seed(
                Guid.Parse("66666666-6666-6666-6666-666666666663"),
                "recovery-is-training",
                "Recovery is training",
                "The quiet half of progress: sleep, breath and the discipline of doing less.",
                "<p>Adaptation happens between sessions. That makes sleep, hydration and deliberate downtime part of the programme, not a reward for finishing it.</p><p>Try ten minutes of breathwork after your next hard session. Keep the pace easy. Notice what changes.</p>",
                "Yuki Tanaka",
                new DateTime(2026, 9, 10),
                imageUrl: null),
            BlogPost.Seed(
                Guid.Parse("66666666-6666-6666-6666-666666666664"),
                "draft-article",
                "Draft article",
                "This article should never appear on the public site.",
                "<p>Draft content.</p>",
                "Kinevor Team",
                new DateTime(2026, 10, 18),
                isPublished: false));

        var enquiry = Lead.Seed(
            Guid.Parse("77777777-7777-7777-7777-777777777771"),
            "Jordan Ellis",
            "jordan.ellis@example.com",
            "+1 (917) 555-0188",
            "Corporate membership",
            "We are looking at memberships for a team of twelve near the Downtown studio. Could you share options?",
            "website",
            LeadLifecycle.Contacted,
            now.AddDays(-2));
        var secondEnquiry = Lead.Seed(
            Guid.Parse("77777777-7777-7777-7777-777777777772"),
            "Priya Shah",
            "priya.shah@example.com",
            null,
            "Personal training",
            "I would like to understand how the quarterly coach check-ins work.",
            "website",
            LeadLifecycle.New,
            now.AddDays(-1));
        db.Leads.AddRange(enquiry, secondEnquiry);

        var trial = TrialBooking.Seed(
            Guid.Parse("88888888-8888-8888-8888-888888888881"),
            now,
            "Noah",
            "Bennett",
            "noah.bennett@example.com",
            "+1 (646) 555-0132",
            day.AddDays(1).AddHours(7),
            FirstClassId,
            "Sunrise HIIT",
            MainBranchId,
            main,
            "Build consistency",
            "First class in more than a year.",
            LeadLifecycle.New,
            now.AddDays(-1));
        db.TrialBookings.Add(trial);

        await db.SaveChangesAsync(ct);

        var stamps = new (Kinevor.Platform.Domain.Common.Entity Entity, DateTime CreatedAt)[]
        {
            (enquiry, now.AddDays(-2)),
            (secondEnquiry, now.AddDays(-1)),
            (trial, now.AddDays(-1)),
        };

        foreach (var stamp in stamps)
        {
            var entry = db.Entry(stamp.Entity).Property(entity => entity.CreatedAt);
            entry.CurrentValue = stamp.CreatedAt;
            entry.IsModified = true;
        }

        await db.SaveChangesAsync(ct);
    }
}
