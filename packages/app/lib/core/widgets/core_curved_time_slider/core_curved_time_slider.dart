import 'dart:math' as math;
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../../../theme.dart';

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

class CoreCurvedTimeSliderState extends State<CoreCurvedTimeSlider> {
  late double _currentValue;
  double? _lastDivisionValue;
  late Offset _center;
  late double _radius;

  @override
  void initState() {
    super.initState();
    _currentValue = widget.value;
    _lastDivisionValue = (_currentValue - 0.5);
  }

  void _handlePanUpdate(DragUpdateDetails details) {
    final RenderBox renderBox = context.findRenderObject() as RenderBox;
    final Size size = renderBox.size;
    _center = Offset(size.width, size.height);  // Bottom-right corner is the center of the quarter circle
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
          GestureDetector(
            onPanUpdate: _handlePanUpdate,
            child: SliderTheme(
              data: SliderThemeData(
                trackShape: CustomCurvedSliderTrackShape(),
                thumbShape: const CustomCurvedSliderThumbShape(thumbRadius: 24),
                trackHeight: 15,
                activeTrackColor: themePrimaryColor,
                inactiveTrackColor: themeSecondaryColor,
                thumbColor: themeSecondaryColor,
              ),
              child: Slider(
                value: _currentValue,
                min: 0,
                max: 24,
                divisions: 48,
                label: null,
                onChanged: null,
              ),
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

class CustomCurvedSliderTrackShape extends SliderTrackShape {
  @override
  Rect getPreferredRect({
    required RenderBox parentBox,
    Offset offset = Offset.zero,
    required SliderThemeData sliderTheme,
    bool isEnabled = false,
    bool isDiscrete = false,
  }) {
    final double trackHeight = sliderTheme.trackHeight!;
    final double trackLeft = offset.dx;
    final double trackTop =
        offset.dy + (parentBox.size.height - trackHeight) / 2;
    final double trackWidth = parentBox.size.width;
    return Rect.fromLTWH(trackLeft, trackTop, trackWidth, trackHeight);
  }

  @override
  void paint(
    PaintingContext context,
    Offset offset, {
    required RenderBox parentBox,
    required SliderThemeData sliderTheme,
    required Animation<double> enableAnimation,
    required TextDirection textDirection,
    required Offset thumbCenter,
    Offset? secondaryOffset,
    bool isDiscrete = false,
    bool isEnabled = false,
  }) {
    if (sliderTheme.trackHeight == null ||
        sliderTheme.activeTrackColor == null ||
        sliderTheme.inactiveTrackColor == null) {
      return;
    }

    final Canvas canvas = context.canvas;
    final Paint activePaint = Paint()
      ..color = sliderTheme.activeTrackColor!
      ..style = PaintingStyle.stroke
      ..strokeWidth = sliderTheme.trackHeight!;
    final Paint inactivePaint = Paint()
      ..color = sliderTheme.inactiveTrackColor!
      ..style = PaintingStyle.stroke
      ..strokeWidth = sliderTheme.trackHeight!;

    final double radius =
        math.min(parentBox.size.width, parentBox.size.height) / 2;
    final Offset center = Offset(
        offset.dx + parentBox.size.width, offset.dy + parentBox.size.height);

    // Draw the inactive (background) arc
    canvas.drawArc(
      Rect.fromCircle(center: center, radius: radius),
      -math.pi / 2,
      -math.pi / 2,
      false,
      inactivePaint,
    );

    // Calculate the angle for the thumb position
    final double angle =
        (1 - (thumbCenter.dx - offset.dx) / parentBox.size.width) * math.pi / 2;

    // Draw the active arc
    canvas.drawArc(
      Rect.fromCircle(center: center, radius: radius),
      -math.pi / 2,
      -angle,
      false,
      activePaint,
    );
  }
}

class CustomCurvedSliderThumbShape extends SliderComponentShape {
  final double thumbRadius;

  const CustomCurvedSliderThumbShape({required this.thumbRadius});

  @override
  Size getPreferredSize(bool isEnabled, bool isDiscrete) {
    return Size.fromRadius(thumbRadius);
  }

  @override
  void paint(
    PaintingContext context,
    Offset center, {
    required Animation<double> activationAnimation,
    required Animation<double> enableAnimation,
    required bool isDiscrete,
    required TextPainter labelPainter,
    required RenderBox parentBox,
    required SliderThemeData sliderTheme,
    required TextDirection textDirection,
    required double value,
    required double textScaleFactor,
    required Size sizeWithOverflow,
  }) {
    final Canvas canvas = context.canvas;

    final double radius =
        math.min(parentBox.size.width, parentBox.size.height) / 2;
    final Offset sliderCenter =
        Offset(parentBox.size.width, parentBox.size.height);

    final double angle = (1 - value) * math.pi / 2;
    final Offset thumbCenter = Offset(
      sliderCenter.dx - radius * math.sin(angle),
      sliderCenter.dy - radius * math.cos(angle),
    );

    final Paint thumbPaint = Paint()
      ..color = sliderTheme.thumbColor!
      ..style = PaintingStyle.fill;

    canvas.drawCircle(thumbCenter, thumbRadius, thumbPaint);
  }
}

