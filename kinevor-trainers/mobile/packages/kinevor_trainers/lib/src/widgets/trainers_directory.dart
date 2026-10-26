import 'dart:async';

import 'package:flutter/material.dart';

import '../data/trainers_api_exception.dart';
import '../data/trainers_repository.dart';
import '../models/trainer.dart';
import '../theme/trainers_strings.dart';
import 'trainer_card.dart';

/// Trainer directory body with debounced search, pull-to-refresh, empty,
/// loading, and error states.
class TrainersDirectory extends StatefulWidget {
  /// Creates a directory bound to [repository].
  const TrainersDirectory({
    super.key,
    required this.repository,
    this.strings = const TrainersStrings(),
    this.onTrainerTap,
    this.pageSize = 50,
  });

  /// Data source for the directory.
  final TrainersRepository repository;

  /// Localized labels.
  final TrainersStrings strings;

  /// Called when a trainer card is tapped.
  final ValueChanged<Trainer>? onTrainerTap;

  /// Number of trainers requested per load.
  final int pageSize;

  @override
  State<TrainersDirectory> createState() => _TrainersDirectoryState();
}

class _TrainersDirectoryState extends State<TrainersDirectory> {
  final TextEditingController _searchController = TextEditingController();
  Timer? _debounce;
  List<Trainer> _trainers = const [];
  String? _error;
  bool _loading = true;
  String _search = '';

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    _debounce?.cancel();
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _load() async {
    if (mounted) {
      setState(() {
        _loading = true;
        _error = null;
      });
    }
    try {
      final page = await widget.repository.getTrainers(
        page: 1,
        pageSize: widget.pageSize,
        search: _search.isEmpty ? null : _search,
      );
      if (!mounted) return;
      setState(() {
        _trainers = page.items;
        _loading = false;
      });
    } on TrainersApiException catch (error) {
      if (!mounted) return;
      setState(() {
        _error = error.message;
        _loading = false;
      });
    } on Exception {
      if (!mounted) return;
      setState(() {
        _error = widget.strings.error;
        _loading = false;
      });
    }
  }

  void _onSearchChanged(String value) {
    setState(() => _search = value);
    _debounce?.cancel();
    _debounce = Timer(const Duration(milliseconds: 350), _load);
  }

  void _clearSearch() {
    _searchController.clear();
    _onSearchChanged('');
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Padding(
          padding: const EdgeInsets.fromLTRB(16, 8, 16, 12),
          child: TextField(
            controller: _searchController,
            onChanged: _onSearchChanged,
            textInputAction: TextInputAction.search,
            decoration: InputDecoration(
              hintText: widget.strings.searchHint,
              prefixIcon: const Icon(Icons.search),
              suffixIcon: _search.isEmpty
                  ? null
                  : IconButton(
                      tooltip: widget.strings.searchHint,
                      icon: const Icon(Icons.close),
                      onPressed: _clearSearch,
                    ),
            ),
          ),
        ),
        Expanded(child: _buildBody()),
      ],
    );
  }

  Widget _buildBody() {
    if (_loading) {
      return const Center(child: CircularProgressIndicator());
    }

    if (_error != null) {
      return _MessageState(
        icon: Icons.cloud_off,
        message: _error!,
        actionLabel: widget.strings.retry,
        onAction: _load,
        onRefresh: _load,
      );
    }

    if (_trainers.isEmpty) {
      return _MessageState(
        icon: Icons.sports,
        message: widget.strings.empty,
        onRefresh: _load,
      );
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView.separated(
        padding: const EdgeInsets.fromLTRB(16, 4, 16, 24),
        itemCount: _trainers.length,
        separatorBuilder: (_, _) => const SizedBox(height: 12),
        itemBuilder: (context, index) {
          final trainer = _trainers[index];
          return TrainerCard(
            trainer: trainer,
            strings: widget.strings,
            onTap: widget.onTrainerTap == null
                ? null
                : () => widget.onTrainerTap!(trainer),
          );
        },
      ),
    );
  }
}

class _MessageState extends StatelessWidget {
  const _MessageState({
    required this.icon,
    required this.message,
    this.actionLabel,
    this.onAction,
    required this.onRefresh,
  });

  final IconData icon;
  final String message;
  final String? actionLabel;
  final VoidCallback? onAction;
  final Future<void> Function() onRefresh;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return RefreshIndicator(
      onRefresh: onRefresh,
      child: LayoutBuilder(
        builder: (context, constraints) => SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          child: ConstrainedBox(
            constraints: BoxConstraints(minHeight: constraints.maxHeight),
            child: Center(
              child: Padding(
                padding: const EdgeInsets.all(32),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(
                      icon,
                      size: 44,
                      color: theme.colorScheme.onSurfaceVariant,
                    ),
                    const SizedBox(height: 12),
                    Text(
                      message,
                      textAlign: TextAlign.center,
                      style: theme.textTheme.bodyMedium?.copyWith(
                        color: theme.colorScheme.onSurfaceVariant,
                      ),
                    ),
                    if (actionLabel != null && onAction != null) ...[
                      const SizedBox(height: 16),
                      FilledButton.tonal(
                        onPressed: onAction,
                        child: Text(actionLabel!),
                      ),
                    ],
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
