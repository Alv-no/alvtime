import 'package:alv_time_app/const.dart';
import 'package:alv_time_app/core/widgets/core_navigation_bar/core_navigation_bar.dart';

import 'package:flutter/material.dart';

import '../../core/widgets/gradient/core_gradient.dart';

class HomeScreenWidget extends StatelessWidget {
  const HomeScreenWidget({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      extendBodyBehindAppBar: true,
      body: CoreTimebasedGradientBackground(
        child: const Text(
          'Faktureringsgrad',
          style: TextStyle(color: Colors.white, fontSize: 40),
        ),
      ),
      appBar: AppBar(
          title: const Text(
        appName,
        style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
      )),
      bottomNavigationBar: const CoreNavigationBar()
    );
  }
}
