import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';

interface ChartItem {
  label: string;
  value: number;
}

interface SvgPoint {
  x: number;
  y: number;
  label: string;
  value: number;
}

interface SvgBar {
  x: number;
  y: number;
  width: number;
  height: number;
  label: string;
  value: number;
  color: string;
}

interface SvgPieSlice {
  path: string;
  label: string;
  value: number;
  percentage: number;
  color: string;
  textX: number;
  textY: number;
}

@Component({
  selector: 'app-svg-chart',
  standalone: false,
  templateUrl: './svg-chart.component.html',
  styleUrl: './svg-chart.component.scss'
})
export class SvgChartComponent implements OnChanges {
  @Input() chartType: 'line' | 'bar' | 'pie' | 'doughnut' | 'area' = 'line';
  @Input() data: ChartItem[] = [];
  @Input() color: string = '#1976d2';

  // Dimensions
  width = 600;
  height = 300;
  paddingLeft = 60;
  paddingRight = 20;
  paddingTop = 20;
  paddingBottom = 40;

  // Render variables
  points: SvgPoint[] = [];
  bars: SvgBar[] = [];
  slices: SvgPieSlice[] = [];
  linePath = '';
  areaPath = '';
  gridLines: number[] = [];
  xLabels: { text: string; x: number }[] = [];
  yLabels: { val: string; y: number }[] = [];

  hoveredItem: any = null;

  private colors = [
    '#1976d2', '#388e3c', '#d32f2f', '#fbc02d', '#7b1fa2',
    '#00897b', '#f57c00', '#455a64', '#e91e63', '#9c27b0'
  ];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data'] || changes['chartType'] || changes['color']) {
      this.generateChart();
    }
  }

  private generateChart(): void {
    if (!this.data || this.data.length === 0) {
      this.reset();
      return;
    }

    if (this.chartType === 'pie' || this.chartType === 'doughnut') {
      this.generatePieChart();
    } else if (this.chartType === 'bar') {
      this.generateBarChart();
    } else {
      this.generateLineOrAreaChart();
    }
  }

  private reset(): void {
    this.points = [];
    this.bars = [];
    this.slices = [];
    this.linePath = '';
    this.areaPath = '';
    this.gridLines = [];
    this.xLabels = [];
    this.yLabels = [];
  }

  private generateLineOrAreaChart(): void {
    this.reset();

    const chartWidth = this.width - this.paddingLeft - this.paddingRight;
    const chartHeight = this.height - this.paddingTop - this.paddingBottom;

    const values = this.data.map(d => d.value);
    const maxVal = Math.max(...values, 10);
    const minVal = 0;
    const valRange = maxVal - minVal;

    // Gridlines (Y-axis)
    const steps = 4;
    for (let i = 0; i <= steps; i++) {
      const val = minVal + (valRange * i) / steps;
      const y = this.height - this.paddingBottom - (chartHeight * i) / steps;
      this.gridLines.push(y);
      this.yLabels.push({ val: this.formatValue(val), y: y + 4 });
    }

    // X Coordinates & points mapping
    const count = this.data.length;
    this.points = this.data.map((d, i) => {
      const x = this.paddingLeft + (chartWidth * i) / Math.max(count - 1, 1);
      const y = this.height - this.paddingBottom - (chartHeight * (d.value - minVal)) / valRange;
      
      // X label
      if (count <= 12 || i % Math.ceil(count / 10) === 0) {
        this.xLabels.push({ text: d.label, x });
      }

      return { x, y, label: d.label, value: d.value };
    });

    // Create Path
    if (this.points.length > 0) {
      let path = `M ${this.points[0].x} ${this.points[0].y}`;
      for (let i = 1; i < this.points.length; i++) {
        path += ` L ${this.points[i].x} ${this.points[i].y}`;
      }
      this.linePath = path;

      // Area Path
      if (this.chartType === 'area') {
        const startX = this.points[0].x;
        const endX = this.points[this.points.length - 1].x;
        const bottomY = this.height - this.paddingBottom;
        this.areaPath = `${path} L ${endX} ${bottomY} L ${startX} ${bottomY} Z`;
      }
    }
  }

  private generateBarChart(): void {
    this.reset();

    const chartWidth = this.width - this.paddingLeft - this.paddingRight;
    const chartHeight = this.height - this.paddingTop - this.paddingBottom;

    const values = this.data.map(d => d.value);
    const maxVal = Math.max(...values, 10);
    const minVal = 0;
    const valRange = maxVal - minVal;

    // Y Gridlines
    const steps = 4;
    for (let i = 0; i <= steps; i++) {
      const val = minVal + (valRange * i) / steps;
      const y = this.height - this.paddingBottom - (chartHeight * i) / steps;
      this.gridLines.push(y);
      this.yLabels.push({ val: this.formatValue(val), y: y + 4 });
    }

    const count = this.data.length;
    const spacing = 0.2; // 20% spacing
    const totalBarWidth = chartWidth / count;
    const barWidth = totalBarWidth * (1 - spacing);

    this.bars = this.data.map((d, i) => {
      const x = this.paddingLeft + (totalBarWidth * i) + (totalBarWidth * spacing / 2);
      const h = (chartHeight * (d.value - minVal)) / valRange;
      const y = this.height - this.paddingBottom - h;

      // X Label centered
      if (count <= 15 || i % Math.ceil(count / 10) === 0) {
        this.xLabels.push({ text: d.label, x: x + barWidth / 2 });
      }

      return {
        x,
        y,
        width: barWidth,
        height: Math.max(h, 2),
        label: d.label,
        value: d.value,
        color: this.colors[i % this.colors.length]
      };
    });
  }

  private generatePieChart(): void {
    this.reset();

    const total = this.data.reduce((sum, item) => sum + item.value, 0);
    if (total === 0) return;

    const centerX = 200;
    const centerY = 200;
    const radius = 150;
    const innerRadius = this.chartType === 'doughnut' ? 90 : 0;

    let accumulatedAngle = -Math.PI / 2; // Start from top 12 o'clock

    this.slices = this.data.map((d, i) => {
      const percentage = d.value / total;
      const angle = percentage * 360 * (Math.PI / 180);

      // Calculations for Arc Path
      const x1 = centerX + radius * Math.cos(accumulatedAngle);
      const y1 = centerY + radius * Math.sin(accumulatedAngle);

      const x2 = centerX + radius * Math.cos(accumulatedAngle + angle);
      const y2 = centerY + radius * Math.sin(accumulatedAngle + angle);

      const largeArc = percentage > 0.5 ? 1 : 0;

      let path = '';
      if (this.chartType === 'doughnut') {
        const ix1 = centerX + innerRadius * Math.cos(accumulatedAngle);
        const iy1 = centerY + innerRadius * Math.sin(accumulatedAngle);
        const ix2 = centerX + innerRadius * Math.cos(accumulatedAngle + angle);
        const iy2 = centerY + innerRadius * Math.sin(accumulatedAngle + angle);

        path = `M ${x1} ${y1} 
                A ${radius} ${radius} 0 ${largeArc} 1 ${x2} ${y2} 
                L ${ix2} ${iy2} 
                A ${innerRadius} ${innerRadius} 0 ${largeArc} 0 ${ix1} ${iy1} Z`;
      } else {
        path = `M ${centerX} ${centerY} L ${x1} ${y1} A ${radius} ${radius} 0 ${largeArc} 1 ${x2} ${y2} Z`;
      }

      // Mid angle for text placement
      const textAngle = accumulatedAngle + (angle / 2);
      const labelRadius = innerRadius + (radius - innerRadius) / 2;
      const textX = centerX + labelRadius * Math.cos(textAngle);
      const textY = centerY + labelRadius * Math.sin(textAngle);

      accumulatedAngle += angle;

      return {
        path,
        label: d.label,
        value: d.value,
        percentage: percentage * 100,
        color: this.colors[i % this.colors.length],
        textX,
        textY
      };
    });
  }

  private formatValue(val: number): string {
    if (val >= 1000000) return (val / 1000000).toFixed(1) + 'M';
    if (val >= 1000) return (val / 1000).toFixed(1) + 'k';
    return val.toString();
  }

  showTooltip(item: any, event: MouseEvent): void {
    this.hoveredItem = item;
  }

  hideTooltip(): void {
    this.hoveredItem = null;
  }
}
