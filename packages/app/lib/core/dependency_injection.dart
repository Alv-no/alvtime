import 'package:alv_time_app/core/authentication.dart';
import 'package:alv_time_app/core/configuration/configuration.dart';
import 'package:alv_time_app/core/configuration/types.dart';
import 'package:flutter/foundation.dart';
import 'package:get_it/get_it.dart';
import 'package:oauth2_client/oauth2_helper.dart';

final _getIt = GetIt.instance;

void configureDependencies() {
  if (kDebugMode || kProfileMode) {
    _getIt.registerSingleton<AppConfiguration>(configurationDevelopment);
  } else {
    _getIt.registerSingleton<AppConfiguration>(configurationProduction);
  }

  _getIt.registerLazySingletonAsync<OAuth2Helper>(createOAuth2Client);
}

final inject = _getIt.get;
final injectAsync = _getIt.getAsync;
final injectAll = _getIt.getAll;
final injectAllAsync = _getIt.getAllAsync;

final getConfiguration = () => _getIt<AppConfiguration>();
