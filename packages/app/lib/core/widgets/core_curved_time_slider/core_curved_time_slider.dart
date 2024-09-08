import 'dart:math' as math;
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../../../theme.dart';
import 'optimized_curved_slider_painter.dart';

class CoreCurvedTimeSlider extends StatefulWidget {
  final double value;
  final ValueChanged<double> onChanged;

  const CoreCurvedTimeSlider({
    super.key,
    required this.value,
    required this.onChanged,
  });

  @override
  CoreCurvedTimeSliderState createState() => CoreCurvedTimeSliderState();
}

class CoreCurvedTimeSliderState extends State<CoreCurvedTimeSlider>
    with SingleTickerProviderStateMixin {
  late double _currentValue;
  double? _lastDivisionValue;
  late Offset _center;
  late double _radius;
  bool _showOverlay = false;
  late AnimationController _animationController;
  late Animation<double> _overlayAnimation;

  @override
  void initState() {
    super.initState();
    _currentValue = widget.value;
    _lastDivisionValue = (_currentValue - 0.5);
    _animationController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 75), // duration for overlay animation
    );
    _overlayAnimation = CurvedAnimation(
      parent: _animationController,
      curve: Curves.easeInOut,
    );
  }

  @override
  void dispose() {
    _animationController.dispose();
    super.dispose();
  }

  void _handlePanUpdate(DragUpdateDetails details) {
    final RenderBox renderBox = context.findRenderObject() as RenderBox;
    final Size size = renderBox.size;
    _center = Offset(size.width,
        size.height); // Bottom-right corner is the center of the quarter circle
    _radius = math.min(size.width, size.height);

    final Offset touchPoint = details.localPosition;
    final double dx = _center.dx - touchPoint.dx;
    final double dy = _center.dy - touchPoint.dy;

    // Calculate the distance from the touch point to the center
    final double distance = math.sqrt(dx * dx + dy * dy);

    // Only update if the touch is within or close to the slider's radius
    if (distance <= _radius * 1.2 && distance > 0) {
      double angle = math.atan2(dx, dy);

      setState(() {
        // Map the angle to the range 0-24
        _currentValue = 24 - (angle / (math.pi / 2) * 24);
        _currentValue = _currentValue.clamp(0.0, 24.0);

        // Snap the current value to the nearest 0.5 increment
        double snappedValue = (2 * _currentValue).round() / 2;

        // If the snapped value changes, trigger haptic feedback
        if (_lastDivisionValue == null || snappedValue != _lastDivisionValue) {
          HapticFeedback.mediumImpact();
          _lastDivisionValue = snappedValue;
        }

        // Update the current value with the snapped value
        _currentValue = snappedValue;
      });

      widget.onChanged(_currentValue);
    }
  }

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      child: Stack(
        alignment: Alignment.center,
        children: [
          RepaintBoundary(
            child: LayoutBuilder(
              builder: (context, constraints) {
                return GestureDetector(
                  onPanStart: (_) {
                    HapticFeedback.mediumImpact();
                    setState(() => _showOverlay = true);
                    _animationController.forward();
                  },
                  onPanEnd: (_) {
                    HapticFeedback.mediumImpact();
                    setState(() => _showOverlay = false);
                    _animationController.reverse();
                  },
                  onPanCancel: () {
                    HapticFeedback.mediumImpact();
                    setState(() => _showOverlay = false);
                    _animationController.reverse();
                  },
                  onPanUpdate: _handlePanUpdate,
                  child: AnimatedBuilder(
                      animation: _overlayAnimation,
                      builder: (context, child) {
                        return CustomPaint(
                          size:
                              Size(constraints.maxWidth, constraints.maxHeight),
                          painter: OptimizedCurvedSliderPainter(
                              value: _currentValue,
                              strokeWidth: 15,
                              thumbRadius: 24,
                              overlayRadius: 48,
                              activeColor: themeSecondaryColor,
                              inactiveColor: themePrimaryColor,
                              thumbColor: themeSecondaryColor,
                              overlayColor:
                                  themeSecondaryColor.withOpacity(0.3),
                              showOverlay: _showOverlay,
                              overlayProgress: _overlayAnimation.value),
                        );
                      }),
                );
              },
            ),
          ),
          Positioned(
              top: 115,
              left: 50,
              child: Container(
                padding: const EdgeInsets.all(8),
                child: Text(
                  _currentValue.toStringAsFixed(1),
                  style: const TextStyle(
                      color: themePrimaryColor,
                      fontWeight: FontWeight.bold,
                      fontSize: 48.0),
                ),
              ))
        ],
      ),
    );
  }
}
