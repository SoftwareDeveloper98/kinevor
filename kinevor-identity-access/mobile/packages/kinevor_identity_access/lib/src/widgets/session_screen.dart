import 'package:flutter/material.dart';

import '../data/identity_access_repository.dart';
import '../data/identity_api_exception.dart';
import '../models/user_account.dart';
import '../theme/identity_access_strings.dart';
import '../theme/kinevor_identity_theme.dart';

/// Session inspector with rotate and sign-out actions.
class SessionScreen extends StatefulWidget {
  /// Creates a session screen for [session].
  const SessionScreen({
    super.key,
    required this.repository,
    required this.session,
    required this.onSignedOut,
    this.onSessionChanged,
    this.strings = const IdentityAccessStrings(),
  });

  /// Data source used for rotation and logout.
  final IdentityAccessRepository repository;

  /// Current session.
  final AuthSession session;

  /// Called after the refresh token has been revoked.
  final VoidCallback onSignedOut;

  /// Called when rotation issues a new session.
  final ValueChanged<AuthSession>? onSessionChanged;

  /// Localized labels.
  final IdentityAccessStrings strings;

  @override
  State<SessionScreen> createState() => _SessionScreenState();
}

class _SessionScreenState extends State<SessionScreen> {
  late AuthSession _session;
  bool _busy = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _session = widget.session;
  }

  @override
  void didUpdateWidget(covariant SessionScreen oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.session != widget.session) _session = widget.session;
  }

  Future<void> _rotate() async {
    if (_busy) return;
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      final refreshed = await widget.repository.refresh(_session.refreshToken);
      if (!mounted) return;
      setState(() {
        _session = refreshed;
        _busy = false;
      });
      widget.onSessionChanged?.call(refreshed);
      _showMessage('Refresh token rotated. The previous token is revoked.');
    } on Exception catch (error) {
      if (!mounted) return;
      setState(() {
        _error = _clean(error);
        _busy = false;
      });
    }
  }

  Future<void> _signOut() async {
    if (_busy) return;
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      await widget.repository.logout(_session.refreshToken);
      if (!mounted) return;
      setState(() => _busy = false);
      widget.onSignedOut();
    } on Exception catch (error) {
      if (!mounted) return;
      setState(() {
        _error = _clean(error);
        _busy = false;
      });
    }
  }

  String _clean(Object error) =>
      error is IdentityApiException ? error.message : error.toString();

  void _showMessage(String message) {
    ScaffoldMessenger.of(context)
      ..hideCurrentSnackBar()
      ..showSnackBar(SnackBar(content: Text(message)));
  }

  @override
  Widget build(BuildContext context) {
    final strings = widget.strings;
    final user = _session.user;
    return Scaffold(
      appBar: AppBar(
        title: Text(strings.sessionTitle),
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 16),
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
              decoration: BoxDecoration(
                color: KinevorIdentityTheme.success.withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(20),
                border: Border.all(
                  color: KinevorIdentityTheme.success.withValues(alpha: 0.4),
                ),
              ),
              child: const Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(
                    Icons.lock_outline,
                    size: 12,
                    color: KinevorIdentityTheme.success,
                  ),
                  SizedBox(width: 4),
                  Text(
                    'AUTHENTICATED',
                    style: TextStyle(
                      fontSize: 9,
                      fontWeight: FontWeight.w800,
                      letterSpacing: 0.8,
                      color: KinevorIdentityTheme.success,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.fromLTRB(20, 8, 20, 32),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              _AccountCard(user: user, strings: strings),
              const SizedBox(height: 14),
              _FactsCard(session: _session, user: user, strings: strings),
              const SizedBox(height: 14),
              _TokenCard(session: _session, strings: strings),
              if (_error != null) ...[
                const SizedBox(height: 14),
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: KinevorIdentityTheme.error.withValues(alpha: 0.12),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Text(
                    _error!,
                    style: Theme.of(context).textTheme.bodySmall?.copyWith(
                      color: KinevorIdentityTheme.error,
                    ),
                  ),
                ),
              ],
              const SizedBox(height: 20),
              FilledButton.icon(
                onPressed: _busy ? null : _rotate,
                style: FilledButton.styleFrom(
                  minimumSize: const Size.fromHeight(50),
                  backgroundColor: KinevorIdentityTheme.accent,
                  foregroundColor: KinevorIdentityTheme.onAccent,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(14),
                  ),
                ),
                icon: _busy
                    ? const SizedBox(
                        width: 16,
                        height: 16,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          color: KinevorIdentityTheme.onAccent,
                        ),
                      )
                    : const Icon(Icons.autorenew, size: 18),
                label: Text(
                  strings.rotate,
                  style: const TextStyle(fontWeight: FontWeight.w700),
                ),
              ),
              const SizedBox(height: 10),
              OutlinedButton.icon(
                onPressed: _busy ? null : _signOut,
                style: OutlinedButton.styleFrom(
                  minimumSize: const Size.fromHeight(50),
                  foregroundColor: KinevorIdentityTheme.text,
                  side: const BorderSide(color: KinevorIdentityTheme.border),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(14),
                  ),
                ),
                icon: const Icon(Icons.logout, size: 18),
                label: Text(
                  strings.signOut,
                  style: const TextStyle(fontWeight: FontWeight.w600),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _AccountCard extends StatelessWidget {
  const _AccountCard({required this.user, required this.strings});

  final UserAccount user;
  final IdentityAccessStrings strings;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            KinevorIdentityTheme.accent.withValues(alpha: 0.26),
            KinevorIdentityTheme.surfaceRaised,
          ],
        ),
        borderRadius: BorderRadius.circular(22),
        border: Border.all(color: KinevorIdentityTheme.border),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            strings.signedInAs.toUpperCase(),
            style: const TextStyle(
              fontSize: 10,
              fontWeight: FontWeight.w800,
              letterSpacing: 1.4,
              color: KinevorIdentityTheme.textMuted,
            ),
          ),
          const SizedBox(height: 12),
          Row(
            children: [
              Container(
                width: 54,
                height: 54,
                decoration: BoxDecoration(
                  color: KinevorIdentityTheme.ink.withValues(alpha: 0.65),
                  shape: BoxShape.circle,
                  border: Border.all(
                    color: KinevorIdentityTheme.accent,
                    width: 1.5,
                  ),
                ),
                alignment: Alignment.center,
                child: Text(
                  user.initials,
                  style: const TextStyle(
                    fontSize: 19,
                    fontWeight: FontWeight.w800,
                    color: KinevorIdentityTheme.accent,
                  ),
                ),
              ),
              const SizedBox(width: 14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      user.displayName,
                      style: Theme.of(context).textTheme.titleLarge,
                    ),
                    const SizedBox(height: 2),
                    Text(
                      user.email,
                      style: Theme.of(context).textTheme.bodySmall?.copyWith(
                        color: KinevorIdentityTheme.textMuted,
                      ),
                    ),
                  ],
                ),
              ),
              Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 10,
                  vertical: 5,
                ),
                decoration: BoxDecoration(
                  color: KinevorIdentityTheme.accent.withValues(alpha: 0.16),
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Text(
                  user.role.toUpperCase(),
                  style: const TextStyle(
                    fontSize: 9,
                    fontWeight: FontWeight.w800,
                    letterSpacing: 0.7,
                    color: KinevorIdentityTheme.accent,
                  ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _FactsCard extends StatelessWidget {
  const _FactsCard({
    required this.session,
    required this.user,
    required this.strings,
  });

  final AuthSession session;
  final UserAccount user;
  final IdentityAccessStrings strings;

  @override
  Widget build(BuildContext context) {
    final lastLogin = user.lastLoginAt;
    return Card(
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 6),
        child: Column(
          children: [
            _FactRow(
              icon: Icons.badge_outlined,
              label: strings.accountId,
              value: user.id,
              mono: true,
            ),
            const Divider(),
            _FactRow(
              icon: Icons.history,
              label: strings.lastSignIn,
              value: lastLogin == null
                  ? strings.never
                  : lastLogin.toLocal().toString().split('.').first,
            ),
            const Divider(),
            _FactRow(
              icon: Icons.verified_user_outlined,
              label: strings.secondFactor,
              value: user.mfaEnabled ? strings.enabled : strings.disabled,
            ),
          ],
        ),
      ),
    );
  }
}

class _FactRow extends StatelessWidget {
  const _FactRow({
    required this.icon,
    required this.label,
    required this.value,
    this.mono = false,
  });

  final IconData icon;
  final String label;
  final String value;
  final bool mono;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 12),
      child: Row(
        children: [
          Icon(icon, size: 17, color: KinevorIdentityTheme.textMuted),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              label,
              style: Theme.of(context).textTheme.bodySmall?.copyWith(
                color: KinevorIdentityTheme.textMuted,
              ),
            ),
          ),
          Flexible(
            child: Text(
              value,
              textAlign: TextAlign.right,
              overflow: TextOverflow.ellipsis,
              style: TextStyle(
                fontSize: 12,
                fontWeight: FontWeight.w600,
                letterSpacing: mono ? 0.4 : null,
                color: KinevorIdentityTheme.text,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _TokenCard extends StatelessWidget {
  const _TokenCard({required this.session, required this.strings});

  final AuthSession session;
  final IdentityAccessStrings strings;

  @override
  Widget build(BuildContext context) {
    final token = session.accessToken;
    final preview = token.length <= 34
        ? token
        : '${token.substring(0, 18)}â€¦${token.substring(token.length - 10)}';
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(18),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                const Icon(
                  Icons.vpn_key_outlined,
                  size: 16,
                  color: KinevorIdentityTheme.accent,
                ),
                const SizedBox(width: 8),
                Text(
                  strings.expiresAt.toUpperCase(),
                  style: const TextStyle(
                    fontSize: 10,
                    fontWeight: FontWeight.w800,
                    letterSpacing: 1.2,
                    color: KinevorIdentityTheme.textMuted,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 8),
            Text(
              session.expiresAt.toLocal().toString().split('.').first,
              style: Theme.of(context).textTheme.titleMedium,
            ),
            const SizedBox(height: 12),
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(10),
              decoration: BoxDecoration(
                color: KinevorIdentityTheme.ink,
                borderRadius: BorderRadius.circular(10),
                border: Border.all(color: KinevorIdentityTheme.border),
              ),
              child: Text(
                preview,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(
                  fontSize: 11,
                  letterSpacing: 0.3,
                  color: KinevorIdentityTheme.textMuted,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
