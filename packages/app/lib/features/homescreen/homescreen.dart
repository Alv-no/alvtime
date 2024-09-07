import 'package:alv_time_app/const.dart';
import 'package:alv_time_app/core/widgets/core_navigation_bar/core_navigation_bar.dart';
import 'package:flutter/material.dart';
import 'package:alv_time_app/features/homescreen/carousel.dart'; // Correct import for Carousel widget

import '../../core/widgets/gradient/core_gradient.dart';

class HomeScreenWidget extends StatelessWidget {
  const HomeScreenWidget({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      extendBodyBehindAppBar: true,
      appBar: AppBar(
        title: const Text(
          appName,
          style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
        ),
        backgroundColor: Colors.transparent,
        elevation: 0,
      ),
      body: Stack(
        children: [
          CoreTimebasedGradientBackground(
            child: Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Text(
                    'Faktureringsgrad',
                    style: TextStyle(color: Colors.white, fontSize: 40),
                  ),
                  const SizedBox(height: 20),
                ],
              ),
            ),
          ),
          Align(
            alignment:
                Alignment.bottomCenter, // Align Carousel at the bottom center
            child: Carousel(), // Carousel widget
          ),
        ],
      ),
      bottomNavigationBar: const CoreNavigationBar(),
    );
  }
}
