import 'package:flutter/material.dart';

import '../data/trainers_api_exception.dart';
import '../data/trainers_repository.dart';
import '../models/trainer.dart';
import '../theme/kinevor_trainers_theme.dart';
import '../theme/trainers_strings.dart';

/// Full trainer profile with rating, contact details, specializations and
/// certifications.
class TrainerDetailPage extends StatefulWidget {
  /// Creates a profile page.
  const TrainerDetailPage({
    super.key,
    required this.repository,
    required this.trainerId,
    this.initialTrainer,
    this.strings = const TrainersStrings(),
  });

  /// Data source for the profile.
  final TrainersRepository repository;

  /// Identifier of the trainer to load.
  final String trainerId;

  /// Optional already-loaded record to render before the refresh completes.
  final Trainer? initialTrainer;

  final TrainersStrings strings;

  @override
  State<TrainerDetailPage> createState() => _TrainerDetailPageState();
}

class _TrainerDetailPageState extends State<TrainerDetailPage> {
  late Future<Trainer> _future;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<Trainer> _load() {
    final initial = widget.initialTrainer;
    if (initial != null) return Future<Trainer>.value(initial);
    return widget.repository.getTrainer(widget.trainerId);
  }

  void _retry() {
    setState(() => _future = widget.repository.getTrainer(widget.trainerId));
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(widget.strings.title)),
      body: FutureBuilder<Trainer>(
        future: _future,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snapshot.hasError) {
            final error = snapshot.error;
            final message = error is TrainersApiException
                ? error.message
                : widget.strings.notFound;
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(32),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(Icons.person_off_outlined, size: 44),
                    const SizedBox(height: 12),
                    Text(message, textAlign: TextAlign.center),
                    const SizedBox(height: 16),
                    FilledButton.tonal(
                      onPressed: _retry,
                      child: Text(widget.strings.retry),
                    ),
                  ],
                ),
              ),
            );
          }
          final trainer = snapshot.data;
          if (trainer == null) {
            return Center(child: Text(widget.strings.notFound));
          }
          return _TrainerProfile(trainer: trainer, strings: widget.strings);
        },
      ),
    );
  }
}

class _TrainerProfile extends StatelessWidget {
  const _TrainerProfile({required this.trainer, required this.strings});

  final Trainer trainer;
  final TrainersStrings strings;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return ListView(
      padding: const EdgeInsets.fromLTRB(16, 8, 16, 32),
      children: [
        Card(
          child: Padding(
            padding: const EdgeInsets.all(20),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _ProfileAvatar(trainer: trainer),
                const SizedBox(width: 16),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        trainer.fullName,
                        style: theme.textTheme.headlineSmall?.copyWith(
                          fontWeight: FontWeight.w800,
                        ),
                      ),
                      const SizedBox(height: 4),
                      Text(
                        strings.yearsExperience(trainer.yearsExperience),
                        style: theme.textTheme.bodySmall?.copyWith(
                          color: theme.colorScheme.onSurfaceVariant,
                        ),
                      ),
                      const SizedBox(height: 8),
                      Row(
                        children: [
                          for (var i = 1; i <= 5; i++)
                            Icon(
                              i <= trainer.rating
                                  ? Icons.star
                                  : (i - 0.5 <= trainer.rating
                                        ? Icons.star_half
                                        : Icons.star_border),
                              color: KinevorTrainersTheme.warning,
                              size: 17,
                            ),
                          const SizedBox(width: 6),
                          Text(
                            '${trainer.rating.toStringAsFixed(1)} '
                            '(${strings.reviews(trainer.totalRatings)})',
                            style: theme.textTheme.bodySmall?.copyWith(
                              color: theme.colorScheme.onSurfaceVariant,
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
        const SizedBox(height: 12),
        Row(
          children: [
            Expanded(
              child: _StatTile(
                icon: Icons.sports,
                label: strings.specializations,
                value: '${trainer.specializations.length}',
              ),
            ),
            const SizedBox(width: 10),
            Expanded(
              child: _StatTile(
                icon: Icons.verified_outlined,
                label: strings.certifications,
                value: '${trainer.certifications.length}',
              ),
            ),
            const SizedBox(width: 10),
            Expanded(
              child: _StatTile(
                icon: Icons.star_border,
                label: strings.statsRating,
                value: trainer.rating.toStringAsFixed(1),
              ),
            ),
          ],
        ),
        const SizedBox(height: 12),
        _SectionCard(
          title: strings.about,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              if (trainer.bio.isNotEmpty) ...[
                Text(
                  trainer.bio,
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: theme.colorScheme.onSurfaceVariant,
                    height: 1.6,
                  ),
                ),
                const SizedBox(height: 16),
              ],
              if (trainer.email.isNotEmpty)
                _ContactRow(icon: Icons.email_outlined, text: trainer.email),
              if (trainer.phone.isNotEmpty)
                _ContactRow(icon: Icons.phone_outlined, text: trainer.phone),
            ],
          ),
        ),
        if (trainer.specializations.isNotEmpty) ...[
          const SizedBox(height: 12),
          _SectionCard(
            title: strings.specializations,
            child: Wrap(
              spacing: 8,
              runSpacing: 8,
              children: [
                for (final specialization in trainer.specializations)
                  Chip(
                    label: Text(specialization),
                    side: BorderSide(color: theme.colorScheme.outline),
                  ),
              ],
            ),
          ),
        ],
        if (trainer.certifications.isNotEmpty) ...[
          const SizedBox(height: 12),
          _SectionCard(
            title: strings.certifications,
            child: Column(
              children: [
                for (final certification in trainer.certifications)
                  ListTile(
                    dense: true,
                    contentPadding: EdgeInsets.zero,
                    leading: const Icon(
                      Icons.verified_outlined,
                      color: KinevorTrainersTheme.success,
                      size: 20,
                    ),
                    title: Text(certification),
                  ),
              ],
            ),
          ),
        ],
      ],
    );
  }
}

class _ProfileAvatar extends StatelessWidget {
  const _ProfileAvatar({required this.trainer});

  final Trainer trainer;

  @override
  Widget build(BuildContext context) {
    final photoUrl = trainer.photoUrl;
    if (photoUrl != null && photoUrl.isNotEmpty) {
      return ClipOval(
        child: Image.network(
          photoUrl,
          width: 72,
          height: 72,
          fit: BoxFit.cover,
          errorBuilder: (_, _, _) => _initials(),
        ),
      );
    }
    return _initials();
  }

  Widget _initials() => Container(
    width: 72,
    height: 72,
    decoration: const BoxDecoration(
      shape: BoxShape.circle,
      gradient: LinearGradient(
        begin: Alignment.topLeft,
        end: Alignment.bottomRight,
        colors: [KinevorTrainersTheme.accentPressed, Color(0xFFFF7A00)],
      ),
    ),
    alignment: Alignment.center,
    child: Text(
      trainer.initials,
      style: const TextStyle(
        color: KinevorTrainersTheme.onAccent,
        fontSize: 26,
        fontWeight: FontWeight.w800,
      ),
    ),
  );
}

class _StatTile extends StatelessWidget {
  const _StatTile({
    required this.icon,
    required this.label,
    required this.value,
  });

  final IconData icon;
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Card(
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: 14, horizontal: 8),
        child: Column(
          children: [
            Icon(icon, size: 20, color: theme.colorScheme.primary),
            const SizedBox(height: 6),
            Text(
              value,
              style: theme.textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.w800,
              ),
            ),
            const SizedBox(height: 2),
            Text(
              label,
              maxLines: 2,
              textAlign: TextAlign.center,
              overflow: TextOverflow.ellipsis,
              style: theme.textTheme.bodySmall?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
                fontSize: 10,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _SectionCard extends StatelessWidget {
  const _SectionCard({required this.title, required this.child});

  final String title;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(18),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(title, style: theme.textTheme.titleLarge),
            const SizedBox(height: 12),
            child,
          ],
        ),
      ),
    );
  }
}

class _ContactRow extends StatelessWidget {
  const _ContactRow({required this.icon, required this.text});

  final IconData icon;
  final String text;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Row(
        children: [
          Icon(icon, size: 18, color: theme.colorScheme.onSurfaceVariant),
          const SizedBox(width: 10),
          Expanded(
            child: Text(
              text,
              style: theme.textTheme.bodyMedium?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
