import 'package:alv_time_app/const.dart';
import 'package:alv_time_app/core/core.dart';
import 'package:alv_time_app/routes.dart';
import 'package:alv_time_app/theme.dart';
import 'package:flutter/material.dart';

void main() {
  runApp(const MyApp());
  configureDependencies();
}

class MyApp extends StatelessWidget {

  const MyApp({super.key});

  // This widget is the root of your application.
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'AlvTime',
      theme: ThemeData(
        primaryColor: themePrimaryColor,
        appBarTheme: const AppBarTheme(
            backgroundColor: Colors.transparent, centerTitle: true),

        colorScheme: const ColorScheme(
            surface: themeSurfaceColor,
            onPrimary: Colors.white,
            onSurface: themePrimaryColor,
            brightness: Brightness.light,
            primary: themePrimaryColor,
            secondary: themeSecondaryColor,
            onSecondary: Colors.white,
            tertiary: themeTetiaryColor,
            error: Colors.grey,
            onError: Colors.white),
        useMaterial3: true,
      ),
      routes: routeConfiguration,
      initialRoute: routeHomeScreen,
    );
  }
}

class MyHomePage extends StatefulWidget {
  const MyHomePage({super.key});

  @override
  State<MyHomePage> createState() => _MyHomePageState();
}

class _MyHomePageState extends State<MyHomePage> {

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text(
          appName,
          style: TextStyle(fontWeight: FontWeight.bold),
        ),
      ),
    );
  }
}
