import 'package:flutter/material.dart';

import '../models/trainer.dart';
import '../theme/kinevor_trainers_theme.dart';
import '../theme/trainers_strings.dart';

/// Card summarizing one trainer in the directory.
class TrainerCard extends StatelessWidget {
  /// Creates a trainer card.
  const TrainerCard({
    super.key,
    required this.trainer,
    this.strings = const TrainersStrings(),
    this.onTap,
  });

  /// Trainer rendered by the card.
  final Trainer trainer;

  /// Localized labels.
  final TrainersStrings strings;

  /// Called when the card is tapped.
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final muted = theme.colorScheme.onSurfaceVariant;
    final accent = theme.colorScheme.primary;

    return Semantics(
      button: onTap != null,
      label: trainer.fullName,
      child: Card(
        child: InkWell(
          onTap: onTap,
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    _TrainerAvatar(trainer: trainer),
                    const SizedBox(width: 14),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            trainer.fullName,
                            style: theme.textTheme.titleMedium?.copyWith(
                              fontWeight: FontWeight.w700,
                              color: theme.colorScheme.onSurface,
                            ),
                          ),
                          const SizedBox(height: 2),
                          Text(
                            strings.yearsExperience(trainer.yearsExperience),
                            style: theme.textTheme.bodySmall?.copyWith(
                              color: muted,
                            ),
                          ),
                          const SizedBox(height: 6),
                          _StarRating(
                            rating: trainer.rating,
                            totalRatings: trainer.totalRatings,
                            strings: strings,
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(width: 8),
                    _StatusBadge(status: trainer.status, strings: strings),
                  ],
                ),
                if (trainer.specializations.isNotEmpty) ...[
                  const SizedBox(height: 12),
                  Wrap(
                    spacing: 6,
                    runSpacing: 6,
                    children: [
                      for (final specialization in trainer.specializations)
                        Container(
                          padding: const EdgeInsets.symmetric(
                            horizontal: 10,
                            vertical: 4,
                          ),
                          decoration: BoxDecoration(
                            color: accent.withValues(alpha: 0.14),
                            borderRadius: BorderRadius.circular(20),
                          ),
                          child: Text(
                            specialization,
                            style: theme.textTheme.labelSmall?.copyWith(
                              color: accent,
                              fontWeight: FontWeight.w700,
                            ),
                          ),
                        ),
                    ],
                  ),
                ],
                if (trainer.bio.isNotEmpty) ...[
                  const SizedBox(height: 10),
                  Text(
                    trainer.bio,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: muted,
                      height: 1.5,
                    ),
                  ),
                ],
                if (trainer.email.isNotEmpty) ...[
                  const SizedBox(height: 10),
                  Row(
                    children: [
                      Icon(Icons.email_outlined, size: 15, color: muted),
                      const SizedBox(width: 6),
                      Expanded(
                        child: Text(
                          trainer.email,
                          style: theme.textTheme.bodySmall?.copyWith(
                            color: muted,
                          ),
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                    ],
                  ),
                ],
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _TrainerAvatar extends StatelessWidget {
  const _TrainerAvatar({required this.trainer});

  final Trainer trainer;

  @override
  Widget build(BuildContext context) {
    final initials = _InitialsAvatar(initials: trainer.initials);
    final photoUrl = trainer.photoUrl;
    if (photoUrl == null || photoUrl.isEmpty) return initials;

    return ClipOval(
      child: Image.network(
        photoUrl,
        width: 56,
        height: 56,
        fit: BoxFit.cover,
        errorBuilder: (_, _, _) => initials,
        loadingBuilder: (_, child, progress) =>
            progress == null ? child : initials,
      ),
    );
  }
}

class _InitialsAvatar extends StatelessWidget {
  const _InitialsAvatar({required this.initials});

  final String initials;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 56,
      height: 56,
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
        initials,
        style: const TextStyle(
          color: KinevorTrainersTheme.onAccent,
          fontSize: 20,
          fontWeight: FontWeight.w800,
        ),
      ),
    );
  }
}

class _StarRating extends StatelessWidget {
  const _StarRating({
    required this.rating,
    required this.totalRatings,
    required this.strings,
  });

  final double rating;
  final int totalRatings;
  final TrainersStrings strings;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Semantics(
      label: strings.rating(rating),
      child: ExcludeSemantics(
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            for (var i = 1; i <= 5; i++)
              Icon(
                i <= rating
                    ? Icons.star
                    : (i - 0.5 <= rating ? Icons.star_half : Icons.star_border),
                color: KinevorTrainersTheme.warning,
                size: 15,
              ),
            const SizedBox(width: 5),
            Text(
              '${rating.toStringAsFixed(1)} (${strings.reviews(totalRatings)})',
              style: theme.textTheme.bodySmall?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
                fontSize: 11,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _StatusBadge extends StatelessWidget {
  const _StatusBadge({required this.status, required this.strings});

  final String status;
  final TrainersStrings strings;

  @override
  Widget build(BuildContext context) {
    if (status.isEmpty) return const SizedBox.shrink();
    final color = KinevorTrainersTheme.statusColor(status);
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.14),
        borderRadius: BorderRadius.circular(20),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(KinevorTrainersTheme.statusIcon(status), size: 12, color: color),
          const SizedBox(width: 4),
          Text(
            strings.status(status),
            style: TextStyle(
              color: color,
              fontSize: 10,
              fontWeight: FontWeight.w700,
            ),
          ),
        ],
      ),
    );
  }
}
