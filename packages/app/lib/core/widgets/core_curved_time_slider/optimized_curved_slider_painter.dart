import 'dart:math' as math;
import 'package:flutter/material.dart';

class OptimizedCurvedSliderPainter extends CustomPainter {
  final double value;
  final Color activeColor;
  final Color inactiveColor;
  final Color thumbColor;
  final Color overlayColor;
  final double strokeWidth;
  final double thumbRadius;
  final double overlayRadius;
  final bool showOverlay;
  final double overlayProgress;

  OptimizedCurvedSliderPainter({
    required this.value,
    required this.activeColor,
    required this.inactiveColor,
    required this.thumbColor,
    required this.overlayColor,
    this.strokeWidth = 15,
    this.thumbRadius = 24,
    this.overlayRadius = 48,
    this.showOverlay = false,
    this.overlayProgress = 0.0,
  });

  @override
  void paint(Canvas canvas, Size size) {
    final double radius = math.min(size.width, size.height) / 2;
    final Offset center = Offset(size.width, size.height);

    final Paint paint = Paint()
      ..style = PaintingStyle.stroke
      ..strokeWidth = strokeWidth
      ..strokeCap = StrokeCap.round;

    // Draw inactive arc
    paint.color = inactiveColor;
    canvas.drawArc(
      Rect.fromCircle(center: center, radius: radius),
      -math.pi / 2,
      -math.pi / 2,
      false,
      paint,
    );

    // Draw active arc
    paint.color = activeColor;
    final double angle = (value / 24) * math.pi / 2;
    canvas.drawArc(
      Rect.fromCircle(center: center, radius: radius),
      math.pi,
      angle,
      false,
      paint,
    );

    // Draw overlay when user handles slider
    if  (showOverlay) {
      final double overlayAngle = -math.pi + angle;
      final Offset overlayCenter = Offset(
        center.dx + radius * math.cos(overlayAngle),
        center.dy + radius * math.sin(overlayAngle),
      );
      paint
        ..style = PaintingStyle.fill
        ..color = overlayColor;

      final double animatedOverlayRadius = thumbRadius + (overlayRadius - thumbRadius) * overlayProgress;

      canvas.drawCircle(overlayCenter, animatedOverlayRadius, paint);
    }

    // Draw thumb
    final double thumbAngle = -math.pi + angle;
    final Offset thumbCenter = Offset(
      center.dx + radius * math.cos(thumbAngle),
      center.dy + radius * math.sin(thumbAngle),
    );
    paint
      ..style = PaintingStyle.fill
      ..color = thumbColor;
    canvas.drawCircle(thumbCenter, thumbRadius, paint);
  }

  @override
  bool shouldRepaint(OptimizedCurvedSliderPainter oldDelegate) {
    return oldDelegate.value != value ||
        oldDelegate.activeColor != activeColor ||
        oldDelegate.inactiveColor != inactiveColor ||
        oldDelegate.thumbColor != thumbColor ||
        oldDelegate.strokeWidth != strokeWidth ||
        oldDelegate.thumbRadius != thumbRadius ||
        oldDelegate.overlayProgress != overlayProgress;
  }
}