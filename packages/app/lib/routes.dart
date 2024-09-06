import 'package:alv_time_app/features/homescreen/homescreen.dart';
import 'package:flutter/widgets.dart';

const routeHomeScreen = "/";
const routeProfileScreen = "/profile";

Map<String, Widget Function(BuildContext)> routeConfiguration = {
  routeHomeScreen: (context) => const HomeScreenWidget(),
};