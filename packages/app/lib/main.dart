import 'package:alv_time_app/const.dart';
import 'package:alv_time_app/core/core.dart';
import 'package:alv_time_app/routes.dart';
import 'package:alv_time_app/theme.dart';
import 'package:flutter/material.dart';
import 'package:oauth2_client/oauth2_helper.dart';

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
        // This is the theme of your application.
        //
        // TRY THIS: Try running your application with "flutter run". You'll see
        // the application has a purple toolbar. Then, without quitting the app,
        // try changing the seedColor in the colorScheme below to Colors.green
        // and then invoke "hot reload" (save your changes or press the "hot
        // reload" button in a Flutter-supported IDE, or press "r" if you used
        // the command line to start the app).
        //
        // Notice that the counter didn't reset back to zero; the application
        // state is not lost during the reload. To reset the state, use hot
        // restart instead.
        //
        // This works for code too, not just values: Most code changes can be
        // tested with just a hot reload.
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

  // This widget is the home page of your application. It is stateful, meaning
  // that it has a State object (defined below) that contains fields that affect
  // how it looks.

  // This class is the configuration for the state. It holds the values (in this
  // case the title) provided by the parent (in this case the App widget) and
  // used by the build method of the State. Fields in a Widget subclass are
  // always marked "final".

  @override
  State<MyHomePage> createState() => _MyHomePageState();
}

class _MyHomePageState extends State<MyHomePage> {
  @override
  Widget build(BuildContext context) {
    // This method is rerun every time setState is called, for instance as done
    // by the _incrementCounter method above.
    //
    // The Flutter framework has been optimized to make rerunning build methods
    // fast, so that you can just rebuild anything that needs updating rather
    // than having to individually change instances of widgets.
    return Scaffold(
      appBar: AppBar(
        // TRY THIS: Try changing the color here to a specific color (to
        // Colors.amber, perhaps?) and trigger a hot reload to see the AppBar
        // change color while the other colors stay the same.
        // Here we take the value from the MyHomePage object that was created by
        // the App.build method, and use it to set our appbar title.
        title: const Text(
          appName,
          style: TextStyle(fontWeight: FontWeight.bold),
        ),
      ),
    );
  }
}
