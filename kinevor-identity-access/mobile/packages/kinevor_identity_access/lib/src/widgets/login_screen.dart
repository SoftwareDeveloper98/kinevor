import 'package:flutter/material.dart';

import '../data/identity_access_repository.dart';
import '../data/identity_api_exception.dart';
import '../models/user_account.dart';
import '../theme/identity_access_strings.dart';
import '../theme/kinevor_identity_theme.dart';

/// Branded login screen backed by [repository].
class LoginScreen extends StatefulWidget {
  /// Creates a login screen.
  const LoginScreen({
    super.key,
    required this.repository,
    required this.onSignedIn,
    this.strings = const IdentityAccessStrings(),
    this.initialEmail = 'admin@kinevor.demo',
    this.initialPassword = 'Kinevor!2026',
  });

  /// Data source used to authenticate.
  final IdentityAccessRepository repository;

  /// Called with the issued session.
  final ValueChanged<AuthSession> onSignedIn;

  /// Localized labels.
  final IdentityAccessStrings strings;

  /// Prefilled e-mail address.
  final String initialEmail;

  /// Prefilled password.
  final String initialPassword;

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  late final TextEditingController _email;
  late final TextEditingController _password;
  bool _busy = false;
  bool _obscure = true;
  String? _error;

  static const _accounts = <String, String>{
    'Admin': 'admin@kinevor.demo',
    'Staff': 'staff@kinevor.demo',
    'Trainer': 'trainer@kinevor.demo',
    'Member': 'member@kinevor.demo',
  };

  @override
  void initState() {
    super.initState();
    _email = TextEditingController(text: widget.initialEmail);
    _password = TextEditingController(text: widget.initialPassword);
  }

  @override
  void dispose() {
    _email.dispose();
    _password.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (_busy) return;
    setState(() {
      _busy = true;
      _error = null;
    });

    try {
      final session = await widget.repository.login(
        _email.text.trim(),
        _password.text,
      );
      if (!mounted) return;
      setState(() => _busy = false);
      widget.onSignedIn(session);
    } on Exception catch (error) {
      if (!mounted) return;
      setState(() {
        _error = error is IdentityApiException
            ? error.message
            : error.toString();
        _busy = false;
      });
    }
  }

  void _useAccount(String email) {
    setState(() {
      _email.text = email;
      _password.text = 'Kinevor!2026';
      _error = null;
    });
  }

  @override
  Widget build(BuildContext context) {
    final strings = widget.strings;
    return Scaffold(
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.fromLTRB(20, 20, 20, 32),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              _TopBar(strings: strings),
              const SizedBox(height: 22),
              _Hero(strings: strings),
              const SizedBox(height: 20),
              _FormCard(
                strings: strings,
                email: _email,
                password: _password,
                obscure: _obscure,
                busy: _busy,
                error: _error,
                onToggleObscure: () => setState(() => _obscure = !_obscure),
                onSubmit: _submit,
              ),
              const SizedBox(height: 18),
              _DemoAccounts(
                strings: strings,
                accounts: _accounts,
                onSelected: _useAccount,
              ),
              const SizedBox(height: 22),
              Text(
                strings.secure,
                textAlign: TextAlign.center,
                style: Theme.of(context).textTheme.labelSmall?.copyWith(
                  color: KinevorIdentityTheme.textMuted,
                  letterSpacing: 0.8,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _TopBar extends StatelessWidget {
  const _TopBar({required this.strings});

  final IdentityAccessStrings strings;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Container(
          width: 38,
          height: 38,
          decoration: BoxDecoration(
            gradient: const LinearGradient(
              colors: [
                KinevorIdentityTheme.accent,
                KinevorIdentityTheme.accentPressed,
              ],
            ),
            borderRadius: BorderRadius.circular(12),
          ),
          alignment: Alignment.center,
          child: const Text(
            'K',
            style: TextStyle(
              color: KinevorIdentityTheme.onAccent,
              fontWeight: FontWeight.w900,
              fontSize: 18,
            ),
          ),
        ),
        const SizedBox(width: 10),
        Expanded(
          child: Text(
            strings.brand,
            style: Theme.of(
              context,
            ).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w800),
          ),
        ),
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
          decoration: BoxDecoration(
            border: Border.all(color: KinevorIdentityTheme.border),
            borderRadius: BorderRadius.circular(20),
          ),
          child: const Text(
            'DEMO',
            style: TextStyle(
              fontSize: 10,
              fontWeight: FontWeight.w800,
              letterSpacing: 1.2,
              color: KinevorIdentityTheme.textMuted,
            ),
          ),
        ),
      ],
    );
  }
}

class _Hero extends StatelessWidget {
  const _Hero({required this.strings});

  final IdentityAccessStrings strings;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(22),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            KinevorIdentityTheme.accent.withValues(alpha: 0.32),
            KinevorIdentityTheme.surfaceRaised,
          ],
        ),
        borderRadius: BorderRadius.circular(24),
        border: Border.all(color: KinevorIdentityTheme.border),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 7,
                height: 7,
                decoration: const BoxDecoration(
                  color: KinevorIdentityTheme.accent,
                  shape: BoxShape.circle,
                ),
              ),
              const SizedBox(width: 7),
              const Text(
                'IDENTITY & ACCESS',
                style: TextStyle(
                  fontSize: 10,
                  fontWeight: FontWeight.w800,
                  letterSpacing: 1.5,
                  color: KinevorIdentityTheme.accent,
                ),
              ),
            ],
          ),
          const SizedBox(height: 14),
          Text(
            strings.welcome,
            style: Theme.of(context).textTheme.headlineMedium?.copyWith(
              fontSize: 30,
              letterSpacing: -0.8,
            ),
          ),
          const SizedBox(height: 6),
          Text(
            strings.subtitle,
            style: Theme.of(context).textTheme.bodyMedium?.copyWith(
              color: KinevorIdentityTheme.textMuted,
              height: 1.5,
            ),
          ),
          const SizedBox(height: 16),
          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: [
              _HeroPill(icon: Icons.shield_outlined, label: 'JWT'),
              _HeroPill(icon: Icons.autorenew, label: 'Rotation'),
              _HeroPill(icon: Icons.key_outlined, label: 'PBKDF2'),
            ],
          ),
        ],
      ),
    );
  }
}

class _HeroPill extends StatelessWidget {
  const _HeroPill({required this.icon, required this.label});

  final IconData icon;
  final String label;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
      decoration: BoxDecoration(
        color: KinevorIdentityTheme.ink.withValues(alpha: 0.5),
        borderRadius: BorderRadius.circular(20),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 13, color: KinevorIdentityTheme.accent),
          const SizedBox(width: 5),
          Text(
            label,
            style: const TextStyle(
              fontSize: 11,
              fontWeight: FontWeight.w600,
              color: KinevorIdentityTheme.text,
            ),
          ),
        ],
      ),
    );
  }
}

class _FormCard extends StatelessWidget {
  const _FormCard({
    required this.strings,
    required this.email,
    required this.password,
    required this.obscure,
    required this.busy,
    required this.error,
    required this.onToggleObscure,
    required this.onSubmit,
  });

  final IdentityAccessStrings strings;
  final TextEditingController email;
  final TextEditingController password;
  final bool obscure;
  final bool busy;
  final String? error;
  final VoidCallback onToggleObscure;
  final VoidCallback onSubmit;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            TextField(
              controller: email,
              keyboardType: TextInputType.emailAddress,
              autofillHints: const [AutofillHints.username],
              decoration: InputDecoration(
                labelText: strings.email,
                prefixIcon: const Icon(Icons.mail_outline),
              ),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: password,
              obscureText: obscure,
              autofillHints: const [AutofillHints.password],
              decoration: InputDecoration(
                labelText: strings.password,
                prefixIcon: const Icon(Icons.lock_outline),
                suffixIcon: IconButton(
                  onPressed: onToggleObscure,
                  tooltip: obscure ? 'Show password' : 'Hide password',
                  icon: Icon(
                    obscure ? Icons.visibility_outlined : Icons.visibility_off,
                  ),
                ),
              ),
              onSubmitted: (_) => onSubmit(),
            ),
            if (error != null) ...[
              const SizedBox(height: 12),
              Container(
                padding: const EdgeInsets.all(10),
                decoration: BoxDecoration(
                  color: KinevorIdentityTheme.error.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Icon(
                      Icons.error_outline,
                      size: 16,
                      color: KinevorIdentityTheme.error,
                    ),
                    const SizedBox(width: 6),
                    Expanded(
                      child: Text(
                        error!,
                        style: Theme.of(context).textTheme.bodySmall?.copyWith(
                          color: KinevorIdentityTheme.error,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ],
            const SizedBox(height: 18),
            FilledButton(
              onPressed: busy ? null : onSubmit,
              style: FilledButton.styleFrom(
                minimumSize: const Size.fromHeight(50),
                backgroundColor: KinevorIdentityTheme.accent,
                foregroundColor: KinevorIdentityTheme.onAccent,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(14),
                ),
              ),
              child: busy
                  ? const SizedBox(
                      width: 20,
                      height: 20,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        color: KinevorIdentityTheme.onAccent,
                      ),
                    )
                  : Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Text(
                          strings.signIn,
                          style: const TextStyle(
                            fontWeight: FontWeight.w700,
                            fontSize: 15,
                          ),
                        ),
                        const SizedBox(width: 6),
                        const Icon(Icons.arrow_forward, size: 18),
                      ],
                    ),
            ),
          ],
        ),
      ),
    );
  }
}

class _DemoAccounts extends StatelessWidget {
  const _DemoAccounts({
    required this.strings,
    required this.accounts,
    required this.onSelected,
  });

  final IdentityAccessStrings strings;
  final Map<String, String> accounts;
  final ValueChanged<String> onSelected;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          strings.demoAccounts.toUpperCase(),
          style: const TextStyle(
            fontSize: 10,
            fontWeight: FontWeight.w800,
            letterSpacing: 1.4,
            color: KinevorIdentityTheme.textMuted,
          ),
        ),
        const SizedBox(height: 8),
        Wrap(
          spacing: 8,
          runSpacing: 8,
          children: [
            for (final entry in accounts.entries)
              ActionChip(
                onPressed: () => onSelected(entry.value),
                avatar: const Icon(Icons.person_outline, size: 15),
                label: Text(entry.key),
                labelStyle: const TextStyle(
                  fontSize: 12,
                  fontWeight: FontWeight.w600,
                ),
                backgroundColor: KinevorIdentityTheme.surfaceRaised,
                side: const BorderSide(color: KinevorIdentityTheme.border),
              ),
          ],
        ),
        const SizedBox(height: 8),
        Text(
          strings.demoPassword,
          style: const TextStyle(
            fontSize: 11,
            color: KinevorIdentityTheme.textMuted,
          ),
        ),
      ],
    );
  }
}
