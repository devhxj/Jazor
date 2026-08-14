//#region src/useConfig.js
function e(e = {}) {
	let t = "#2D353C", n = "#A1A1A1", r = "#e1e5e8", i = "#CCCCCC", a = "#4A4A4A", o = "#FFFFFF", s = "#fafafa", c = "#1f77b4", l = "#d62728", u = "#2ca02c", d = "#42d392", f = "#FFD055", p = e.colorBackground ?? o, m = e.colorTextPrimary ?? t, h = e.colorTextSecondary ?? n, g = e.colorGrid ?? r, _ = e.colorBorder ?? r, v = { translations: {
		keyboardNavigation: "Use the left and right arrow keys to move between data points.",
		tableAvailable: "A data table for this chart is available below.",
		tableCaption: "Chart data table"
	} }, y = {
		_6: 6,
		_8: 8,
		_10: 10,
		_12: 12,
		_14: 14,
		_16: 16,
		_18: 18,
		_20: 20,
		_24: 24,
		_32: 32,
		_48: 48
	}, b = y._6, x = {
		LINE: "line",
		SQUARE: "square",
		ROUND: "round",
		CIRCLE: "circle",
		CURVED: "curved",
		BAR: "bar",
		STAR: "star"
	}, S = {
		LEFT: "left",
		CENTER: "center",
		RIGHT: "right",
		BOTTOM: "bottom",
		TOP: "top"
	}, ee = {
		ASC: "asc",
		DESC: "desc",
		NONE: "none"
	}, C = { threshold: 1095 }, w = { enable: !1 }, T = {
		showValueFirst: !0,
		usePercentageParens: !0,
		useValueParens: !1
	}, E = {
		text: "",
		color: m,
		fontSize: y._20,
		bold: !0,
		textAlign: S.CENTER,
		paddingLeft: 0,
		paddingRight: 0,
		subtitle: {
			color: h,
			text: "",
			fontSize: y._16,
			bold: !1
		}
	}, D = {
		show: !0,
		bold: !1,
		backgroundColor: p,
		color: m,
		fontSize: y._14,
		selectAllToggle: {
			show: !1,
			backgroundColor: _,
			color: m
		}
	}, O = {
		show: !0,
		color: m,
		backgroundColor: p,
		fontSize: y._14,
		customFormat: null,
		borderRadius: 4,
		borderColor: _,
		borderWidth: 1,
		backgroundOpacity: 100,
		position: S.CENTER,
		offsetX: 0,
		offsetY: 24,
		smooth: !0,
		backdropFilter: !0,
		smoothForce: .18,
		smoothSnapThreshold: .25,
		teleportTo: "body"
	}, k = {
		enable: !0,
		pauseOnLoad: !0,
		pauseOnDatasetChange: !1,
		activationDelayMs: 300
	}, A = {
		enable: !1,
		locale: "en",
		useUTC: !1,
		januaryAsYear: !1,
		options: {
			year: "yyyy",
			month: "MMM 'yy",
			day: "dd MMM",
			hour: "HH:mm",
			minute: "HH:mm:ss",
			second: "HH:mm:ss"
		}
	}, j = ({ tooltip: e = !1, pdf: t = !1, csv: n = !1, img: r = !1, table: i = !1, labels: a = !1, fullscreen: o = !1, sort: s = !1, stack: c = !1, animation: l = !1, annotator: u = !1, svg: d = !1, zoom: f = !1, altCopy: p = !1 }) => {
		let m = {
			open: "Open options",
			close: "Close options",
			tooltip: "Toggle tooltip",
			pdf: "Download PDF",
			csv: "Download CSV",
			img: "Download PNG",
			table: "Toggle table",
			labels: "Toggle labels",
			fullscreen: "Toggle fullscreen",
			sort: "Toggle sort",
			stack: "Toggle stack mode",
			animation: "Toggle animation",
			annotator: "Toggle annotator",
			svg: "Download SVG",
			zoom: "Toggle zoom lock",
			altCopy: "Copy alt text"
		};
		return e || delete m.tooltip, t || delete m.pdf, n || delete m.csv, r || delete m.img, i || delete m.table, a || delete m.labels, o || delete m.fullscreen, s || delete m.sort, c || delete m.stack, l || delete m.animation, u || delete m.annotator, d || delete m.svg, f || delete m.zoom, {
			show: !0,
			showOnChartHover: !1,
			keepStateOnChartLeave: !0,
			position: "right",
			buttons: {
				tooltip: e,
				pdf: t,
				csv: n,
				img: r,
				table: i,
				labels: a,
				fullscreen: o,
				sort: s,
				stack: c,
				animation: l,
				annotator: u,
				svg: d,
				zoom: f,
				altCopy: p
			},
			callbacks: {
				animation: null,
				annotator: null,
				csv: null,
				fullscreen: null,
				img: null,
				labels: null,
				pdf: null,
				sort: null,
				stack: null,
				table: null,
				tooltip: null,
				svg: null,
				zoom: null,
				altCopy: null
			},
			buttonTitles: m,
			print: {
				scale: 2,
				orientation: "auto",
				overflowTolerance: .2
			},
			useCursorPointer: !1
		};
	}, M = (e) => ({
		top: e[0],
		right: e[1],
		bottom: e[2],
		left: e[3]
	}), N = {
		backgroundColor: p,
		color: m,
		outline: "none"
	}, P = {
		backgroundColor: p,
		color: m,
		outline: "none"
	}, F = {
		show: !1,
		responsiveBreakpoint: 400
	}, I = {
		show: !0,
		autoFit: !1,
		color: i,
		highlightColor: a,
		fontSize: y._14,
		useResetSlot: !1,
		startIndex: null,
		endIndex: null,
		enableRangeHandles: !0,
		enableSelectionDrag: !0,
		focusOnDrag: !1,
		focusRangeRatio: .2,
		maxWidth: null,
		keepState: !1
	}, L = {
		show: !1,
		selectedColor: c,
		selectedColorOpacity: .2,
		indicatorColor: t,
		verticalHandles: !1,
		compact: !0,
		frameColor: n,
		additionalHeight: 0,
		handleIconColor: null,
		handleBorderWidth: 1,
		handleBorderColor: null,
		handleFill: null,
		handleWidth: 20,
		handleType: "grab"
	}, R = {
		...L,
		smooth: !1,
		lineColor: t,
		selectionRadius: 2,
		merged: !1
	}, z = {
		devHints: w,
		loading: !1,
		readonly: !1,
		editing: !1,
		theme: "",
		customPalette: [],
		transitions: k,
		userOptions: j({
			tooltip: !1,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !1,
			labels: !1,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right arrow keys to adjust the selected datapoint.",
				topOfHill: "Top of the hill"
			}
		},
		events: {
			edit: null,
			save: null,
			cancel: null,
			change: null,
			dragStart: null,
			dragEnd: null,
			datapointEnter: null,
			datapointLeave: null,
			selectDatapoint: null
		},
		interaction: {
			keyboardStep: .01,
			peakTolerance: .005
		},
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				width: 800,
				height: 300,
				title: E,
				toolbar: {
					show: !0,
					status: {
						lastUpdated: "Last updated just now",
						editInstruction: "Drag each dot to adjust its position",
						color: m,
						fontSize: y._14,
						bold: !1,
						lineHeight: 1.3
					},
					buttons: { translations: {
						edit: "Edit",
						cancel: "Cancel",
						save: "Save"
					} }
				},
				layout: {
					hill: {
						geometry: {
							horizontalPaddingRatio: .05,
							topPaddingRatio: .15,
							bottomPaddingRatio: .15,
							curvature: .7
						},
						baseline: {
							show: !0,
							stroke: i,
							strokeWidth: 1,
							strokeDasharray: 0
						},
						midline: {
							show: !0,
							stroke: i,
							strokeWidth: 1,
							strokeDasharray: "2.5 3.5"
						},
						curve: {
							stroke: n,
							strokeWidth: 1.5,
							strokeDasharray: 0
						}
					},
					plots: {
						radius: 10,
						hitRadius: 10,
						stroke: p,
						strokeWidth: 2,
						mutedOpacity: .4,
						disabledOpacity: .4,
						shadow: {
							show: !0,
							color: m,
							offsetX: 0,
							offsetY: .5,
							blur: .4
						},
						stacking: {
							show: !0,
							overlapThresholdRatio: .8,
							gap: 5,
							overflow: {
								show: !0,
								transitionDuration: 300,
								marker: {
									radius: 10,
									stroke: p,
									strokeWidth: 1,
									labelColor: m,
									labelOffsetY: 0,
									fontSize: y._14,
									bold: !1,
									fill: i
								},
								menu: {
									width: 220,
									maxHeight: 220,
									backgroundColor: p,
									color: m,
									borderColor: "transparent",
									borderRadius: 3,
									title: ""
								}
							}
						},
						dragMarker: {
							show: !0,
							strokeWidth: 1.5,
							crossPath: "M-5 0H5M0-5V5",
							positionIndicator: {
								show: !0,
								useSerieColor: !0,
								color: r,
								strokeWidth: 1,
								strokeDasharray: "2 4",
								circle: {
									show: !0,
									radius: 3,
									stroke: p,
									strokeWidth: 1
								},
								value: {
									show: !0,
									offsetY: 0,
									fontSize: y._12,
									useSerieColor: !1,
									color: m,
									formatter: null,
									rounding: 0
								}
							}
						}
					},
					labels: {
						item: {
							ellipsisThresholdChars: 24,
							show: !0,
							useSerieColor: !1,
							color: m,
							fontSize: y._14,
							bold: !1,
							offsetX: 0,
							offsetY: 0,
							autoSideThreshold: .75,
							stroke: p,
							strokeWidth: 5
						},
						phases: {
							show: !0,
							color: h,
							fontSize: y._14,
							bold: !0,
							letterSpacing: "0.1em",
							offsetY: 0,
							left: { text: "FIGURING THINGS OUT" },
							right: { text: "MAKING IT HAPPEN" }
						}
					},
					stackbar: {
						show: !0,
						paddingTop: 0,
						paddingBottom: 12,
						height: 14,
						stroke: p,
						strokeWidth: 1.5,
						gutterColor: r,
						label: {
							show: !0,
							color: m,
							fontSize: y._12,
							formatter: null
						}
					}
				}
			}
		}
	}, B = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		loading: !1,
		debug: !1,
		theme: "",
		responsive: !1,
		a11y: v,
		transitions: k,
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		customPalette: [],
		useCssAnimation: !1,
		table: {
			...F,
			useDialog: !1,
			columnNames: {
				period: "Period",
				total: "Total"
			},
			th: N,
			td: {
				...P,
				roundingValue: 0
			}
		},
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			labels: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				height: 500,
				width: 800,
				padding: M([
					12,
					12,
					12,
					12
				]),
				title: E,
				legend: {
					...D,
					position: "bottom"
				},
				zoom: {
					...I,
					minimap: L,
					preview: {
						enable: !0,
						fill: "#CCCCCC50",
						stroke: "#6A6A6A",
						strokeWidth: 2,
						strokeDasharray: 0
					},
					useDefaultFormat: !0,
					timeFormat: "yyyy-MM-dd HH:mm:ss",
					customFormat: null
				},
				highlighter: {
					color: m,
					opacity: 5,
					useLine: !1,
					lineDasharray: 2,
					lineWidth: 1
				},
				tooltip: {
					...O,
					showValue: !0,
					showPercentage: !0,
					roundingValue: 0,
					roundingPercentage: 0,
					showTimeLabel: !0,
					showTotal: !1,
					totalTranslation: "Total",
					useDefaultTimeFormat: !0,
					timeFormat: "yyyy-MM-dd HH:mm:ss"
				},
				grid: {
					scale: {
						ticks: 10,
						scaleMin: null,
						scaleMax: null
					},
					frame: {
						show: !1,
						stroke: g,
						strokeWidth: 2,
						strokeLinecap: "round",
						strokeLinejoin: "round",
						strokeDasharray: 0
					},
					x: {
						showAxis: !0,
						showHorizontalLines: !1,
						axisColor: g,
						linesColor: g,
						linesThickness: 1,
						linesStrokeDasharray: 0,
						axisThickness: 2,
						axisName: {
							show: !0,
							text: "",
							fontSize: y._14,
							color: m,
							bold: !1,
							offsetY: 0
						},
						timeLabels: {
							show: !0,
							values: [],
							datetimeFormatter: A,
							offsetY: 0,
							rotation: 0,
							autoRotate: {
								enable: !0,
								angle: -30
							},
							fontSize: y._14,
							color: m,
							bold: !1,
							showOnlyFirstAndLast: !1,
							showOnlyAtModulo: !1,
							modulo: 12
						}
					},
					y: {
						position: "left",
						showAxis: !0,
						showVerticalLines: !1,
						linesColor: g,
						linesThickness: 1,
						linesStrokeDasharray: 0,
						axisColor: g,
						axisThickness: 2,
						axisName: {
							show: !0,
							text: "",
							fontSize: y._14,
							color: m,
							bold: !1,
							offsetX: 0
						},
						axisLabels: {
							formatter: null,
							show: !0,
							color: m,
							fontSize: y._14,
							bold: !1,
							rounding: 0
						}
					}
				},
				lines: {
					useArea: !0,
					smooth: !1,
					areaOpacity: .5,
					distributed: !1,
					showDistributedPercentage: !0,
					strokeWidth: 1,
					gradient: {
						show: !0,
						intensity: 40
					},
					path: {
						useSerieColor: !0,
						stroke: o
					},
					dot: {
						hideAboveMaxSerieLength: 62,
						useSerieColor: !0,
						fill: o,
						stroke: o,
						strokeWidth: .5,
						radius: 4
					},
					totalValues: {
						show: !0,
						offsetY: 0,
						fontSize: y._16,
						bold: !0,
						color: m
					},
					dataLabels: {
						hideAboveMaxSerieLength: 62,
						offsetY: 0,
						show: !0,
						hideEmptyValues: !0,
						hideUnderValue: null,
						hideEmptyPercentages: !0,
						hideUnderPercentage: null,
						color: m,
						fontSize: y._14,
						bold: !1,
						rounding: 0,
						prefix: "",
						suffix: "",
						formatter: null
					}
				}
			}
		}
	}, te = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		loading: !1,
		debug: !1,
		theme: "",
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		transitions: k,
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		customPalette: [],
		useCssAnimation: !1,
		orientation: "vertical",
		table: {
			...F,
			useDialog: !1,
			columnNames: {
				period: "Period",
				total: "Total"
			},
			th: N,
			td: {
				...P,
				roundingValue: 0
			}
		},
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			labels: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				height: 500,
				width: 800,
				padding: M([
					12,
					12,
					12,
					12
				]),
				title: E,
				legend: {
					...D,
					position: "bottom"
				},
				zoom: {
					...I,
					minimap: L,
					preview: {
						enable: !0,
						fill: "#CCCCCC50",
						stroke: "#6A6A6A",
						strokeWidth: 2,
						strokeDasharray: 0
					},
					useDefaultFormat: !0,
					timeFormat: "yyyy-MM-dd HH:mm:ss",
					customFormat: null
				},
				tooltip: {
					...O,
					showValue: !0,
					showPercentage: !0,
					roundingValue: 0,
					roundingPercentage: 0,
					showTimeLabel: !0,
					showTotal: !1,
					totalTranslation: "Total",
					useDefaultTimeFormat: !0,
					timeFormat: "yyyy-MM-dd HH:mm:ss"
				},
				highlighter: {
					color: m,
					opacity: 5
				},
				bars: {
					gapRatio: .5,
					distributed: !1,
					showDistributedPercentage: !0,
					borderRadius: 0,
					strokeWidth: 1,
					gradient: {
						show: !0,
						intensity: 20
					},
					totalValues: {
						show: !0,
						offsetY: 0,
						offsetX: 0,
						fontSize: y._16,
						bold: !1,
						color: m
					},
					dataLabels: {
						show: !0,
						hideEmptyValues: !1,
						hideUnderValue: null,
						hideEmptyPercentages: !1,
						hideUnderPercentage: null,
						adaptColorToBackground: !0,
						color: m,
						fontSize: y._14,
						bold: !1,
						rounding: 0,
						prefix: "",
						suffix: "",
						formatter: null
					}
				},
				grid: {
					scale: {
						ticks: 10,
						scaleMin: null,
						scaleMax: null
					},
					frame: {
						show: !1,
						stroke: g,
						strokeWidth: 2,
						strokeLinecap: "round",
						strokeLinejoin: "round",
						strokeDasharray: 0
					},
					x: {
						showAxis: !0,
						showHorizontalLines: !1,
						axisColor: g,
						linesColor: g,
						linesThickness: 1,
						linesStrokeDasharray: 0,
						axisThickness: 2,
						axisName: {
							show: !0,
							text: "",
							fontSize: y._14,
							color: m,
							bold: !1,
							offsetY: 0
						},
						timeLabels: {
							show: !0,
							values: [],
							datetimeFormatter: A,
							offsetY: 0,
							rotation: 0,
							autoRotate: {
								enable: !0,
								angle: -30
							},
							fontSize: y._14,
							color: m,
							bold: !1,
							showOnlyFirstAndLast: !1,
							showOnlyAtModulo: !1,
							modulo: 12
						}
					},
					y: {
						position: "left",
						showAxis: !0,
						showVerticalLines: !1,
						linesColor: g,
						linesThickness: 1,
						linesStrokeDasharray: 0,
						axisColor: g,
						axisThickness: 2,
						axisName: {
							show: !0,
							text: "",
							fontSize: y._14,
							color: m,
							bold: !1,
							offsetX: 0
						},
						axisLabels: {
							formatter: null,
							show: !0,
							color: m,
							fontSize: y._14,
							bold: !1,
							rounding: 0
						}
					}
				}
			}
		}
	}, V = {
		devHints: w,
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		theme: "",
		responsive: !1,
		loading: !1,
		a11y: v,
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		responsiveProportionalSizing: !0,
		customPalette: [],
		useCssAnimation: !1,
		downsample: C,
		transitions: k,
		chart: {
			fontFamily: "inherit",
			backgroundColor: p,
			color: m,
			height: 600,
			width: 1e3,
			annotations: [{
				show: !1,
				yAxis: {
					yTop: null,
					yBottom: null,
					label: {
						text: "",
						textAnchor: "start",
						position: "start",
						offsetX: 0,
						offsetY: 0,
						padding: M([
							12,
							12,
							12,
							12
						]),
						border: {
							stroke: o,
							strokeWidth: 1,
							rx: 0,
							ry: 0
						},
						fontSize: 14,
						color: m,
						backgroundColor: g
					},
					line: {
						stroke: t,
						strokeWidth: 1,
						strokeDasharray: 0
					},
					area: {
						fill: g,
						opacity: 30
					}
				}
			}],
			zoom: {
				...I,
				minimap: R,
				preview: {
					enable: !0,
					fill: "#CCCCCC50",
					stroke: "#6A6A6A",
					strokeWidth: 2,
					strokeDasharray: 0
				},
				useDefaultFormat: !0,
				timeFormat: "yyyy-MM-dd HH:mm:ss",
				customFormat: null
			},
			padding: M([
				12,
				12,
				6,
				6
			]),
			highlighter: {
				color: m,
				opacity: 5,
				useLine: !1,
				lineDasharray: 2,
				lineWidth: 1,
				crosshairs: {
					show: !1,
					stroke: t,
					strokeWidth: 1,
					strokeDasharray: 3,
					stopOnPoint: !1,
					dot: {
						radius: 2,
						fill: t,
						stroke: o,
						strokeWidth: 1
					}
				}
			},
			highlightArea: {
				show: !1,
				from: 0,
				to: 0,
				color: t,
				opacity: 20,
				caption: {
					text: "",
					fontSize: y._10,
					color: m,
					bold: !1,
					offsetY: 0,
					width: "auto",
					padding: 3,
					textAlign: S.CENTER
				}
			},
			timeTag: {
				show: !1,
				backgroundColor: g,
				color: m,
				fontSize: y._12,
				circleMarker: {
					radius: 3,
					color: m
				},
				useDefaultFormat: !0,
				timeFormat: "yyyy-MM-dd HH:mm:ss",
				customFormat: null
			},
			grid: {
				stroke: g,
				showVerticalLines: !1,
				showHorizontalLines: !1,
				position: "middle",
				frame: {
					show: !1,
					stroke: g,
					strokeWidth: 2,
					strokeLinecap: "round",
					strokeLinejoin: "round",
					strokeDasharray: 0
				},
				labels: {
					show: !0,
					color: m,
					fontSize: y._16,
					axis: {
						yLabel: "",
						yLabelOffsetX: 0,
						xLabel: "",
						xLabelOffsetY: 0,
						fontSize: y._14
					},
					zeroLine: { show: !0 },
					xAxis: {
						showBaseline: !1,
						showCrosshairs: !0,
						crosshairsAlwaysAtZero: !1,
						crosshairSize: 6,
						commonScaleSteps: 10,
						useNiceScale: !0,
						scaleMin: null,
						scaleMax: null,
						rounding: 1,
						formatter: null,
						reverse: !1
					},
					yAxis: {
						reverse: !1,
						position: "left",
						showBaseline: !0,
						showCrosshairs: !0,
						crosshairSize: 6,
						commonScaleSteps: 10,
						useIndividualScale: !1,
						useNiceScale: !1,
						stacked: !1,
						gap: 12,
						labelWidth: 64,
						formatter: null,
						scaleMin: null,
						scaleMax: null,
						groupColor: null,
						scaleLabelOffsetX: 0,
						scaleValueOffsetX: 0,
						rounding: 1,
						serieNameFormatter: null
					},
					xAxisLabels: {
						color: m,
						show: !0,
						values: [],
						datetimeFormatter: A,
						fontSize: y._14,
						showOnlyFirstAndLast: !1,
						showOnlyAtModulo: !1,
						modulo: 12,
						yOffset: 24,
						rotation: 0,
						autoRotate: {
							enable: !0,
							angle: -30
						}
					}
				}
			},
			comments: {
				show: !0,
				showInTooltip: !0,
				width: 200,
				offsetX: 0,
				offsetY: 0
			},
			labels: {
				fontSize: y._10,
				prefix: "",
				suffix: ""
			},
			legend: {
				color: m,
				show: !0,
				fontSize: y._14,
				position: "bottom",
				selectAllToggle: {
					show: !1,
					backgroundColor: _,
					color: m
				}
			},
			title: {
				...E,
				show: !0
			},
			tooltip: {
				...O,
				showTimeLabel: !0,
				showValue: !0,
				showPercentage: !0,
				roundingValue: 0,
				roundingPercentage: 0,
				useDefaultTimeFormat: !0,
				timeFormat: "yyyy-MM-dd HH:mm:ss"
			},
			userOptions: j({
				tooltip: !0,
				pdf: !0,
				csv: !0,
				img: !0,
				table: !0,
				labels: !0,
				fullscreen: !0,
				stack: !0,
				annotator: !0,
				svg: !0
			})
		},
		bar: {
			showTransition: !0,
			transitionDurationMs: 300,
			borderRadius: 2,
			useGradient: !0,
			periodGap: .1,
			innerGap: 0,
			border: {
				useSerieColor: !1,
				strokeWidth: 0,
				stroke: o
			},
			labels: {
				show: !1,
				offsetY: -6,
				offsetX: 0,
				rotation: 0,
				textAnchor: null,
				alwaysOnTop: !1,
				rounding: 0,
				color: m,
				formatter: null
			},
			serieName: {
				show: !1,
				offsetY: -6,
				useAbbreviation: !0,
				abbreviationSize: 3,
				useSerieColor: !0,
				color: m,
				bold: !1
			}
		},
		line: {
			showTransition: !0,
			transitionDurationMs: 300,
			radius: 3,
			useGradient: !0,
			strokeWidth: 3,
			cutNullValues: !1,
			interLine: {
				pairs: [],
				colors: [],
				fillOpacity: .25
			},
			dot: {
				selectedRadius: 4.5,
				hideAboveMaxSerieLength: 62,
				useSerieColor: !0,
				fill: o,
				strokeWidth: .5
			},
			labels: {
				show: !1,
				offsetY: -6,
				offsetX: 0,
				rotation: 0,
				rounding: 0,
				color: m,
				formatter: null,
				textAnchor: null,
				alwaysOnTop: !1
			},
			area: {
				useGradient: !0,
				opacity: 30
			},
			tag: {
				followValue: !0,
				formatter: null,
				fontSize: y._14
			}
		},
		plot: {
			showTransition: !0,
			transitionDurationMs: 300,
			radius: 3,
			useGradient: !0,
			dot: {
				useSerieColor: !0,
				fill: o,
				strokeWidth: .5
			},
			labels: {
				show: !1,
				offsetY: -6,
				offsetX: 0,
				rotation: 0,
				rounding: 0,
				color: m,
				formatter: null,
				textAnchor: null,
				alwaysOnTop: !1
			},
			tag: {
				followValue: !0,
				formatter: null,
				fontSize: y._14
			}
		},
		table: {
			useDialog: !1,
			responsiveBreakpoint: 400,
			rounding: 0,
			sparkline: !0,
			showSum: !0,
			columnNames: {
				period: "Period",
				total: "Total"
			},
			th: N,
			td: P,
			useDefaultTimeFormat: !0,
			timeFormat: "yyyy-MM-dd HH:mm:ss"
		},
		showTable: !1
	}, H = {
		devHints: w,
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		type: "classic",
		loading: !1,
		pie: !1,
		autoSize: !0,
		responsive: !1,
		theme: "",
		customPalette: [],
		useCssAnimation: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		serieToggleAnimation: {
			show: !0,
			durationMs: 500
		},
		startAnimation: {
			show: !1,
			durationMs: 1e3,
			staggerMs: 50
		},
		useBlurOnHover: !0,
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			labels: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		translations: {
			total: "Total",
			average: "Average"
		},
		table: {
			...F,
			useDialog: !1,
			th: N,
			td: {
				...P,
				roundingValue: 0,
				roundingPercentage: 0
			},
			columnNames: {
				series: "Series",
				value: "Value",
				percentage: "Percentage"
			}
		},
		style: {
			fontFamily: "inherit",
			chart: {
				useGradient: !0,
				gradientIntensity: 40,
				backgroundColor: p,
				color: m,
				padding: M([
					0,
					0,
					0,
					0
				]),
				width: 512,
				height: 360,
				layout: {
					curvedMarkers: !1,
					labels: {
						dataLabels: {
							...T,
							show: !0,
							useLabelSlots: !1,
							hideUnderValue: 3,
							smallArcClusterThreshold: 8,
							smallArcClusterFontSize: y._12,
							oneLine: !1,
							prefix: "",
							suffix: ""
						},
						value: {
							rounding: 0,
							show: !0,
							formatter: null
						},
						percentage: {
							show: !0,
							color: m,
							bold: !0,
							fontSize: y._18,
							minFontSize: b,
							rounding: 0,
							formatter: null
						},
						name: {
							show: !0,
							color: m,
							bold: !1,
							fontSize: y._14,
							minFontSize: b
						},
						hollow: {
							show: !0,
							total: {
								show: !0,
								bold: !1,
								fontSize: y._18,
								color: m,
								text: "Total",
								offsetY: 0,
								value: {
									color: m,
									fontSize: y._18,
									bold: !0,
									suffix: "",
									prefix: "",
									offsetY: 0,
									rounding: 0,
									formatter: null
								}
							},
							average: {
								show: !0,
								bold: !1,
								fontSize: y._18,
								color: h,
								text: "Average",
								offsetY: 0,
								value: {
									color: m,
									fontSize: y._18,
									bold: !0,
									suffix: "",
									prefix: "",
									offsetY: 0,
									rounding: 0,
									formatter: null
								}
							}
						}
					},
					donut: {
						radiusRatio: .3,
						strokeWidth: 64,
						borderWidth: 1,
						useShadow: !1,
						shadowColor: t,
						emptyFill: g,
						selectedColor: "#0000001A",
						borderColorAuto: !0,
						borderColor: _
					}
				},
				comments: {
					show: !0,
					showInTooltip: !0,
					width: 100,
					offsetY: 0,
					offsetX: 0
				},
				legend: {
					...D,
					...T,
					roundingValue: 0,
					roundingPercentage: 0,
					showPercentage: !0,
					showValue: !0,
					position: "bottom"
				},
				tooltip: {
					...O,
					...T,
					showValue: !0,
					showPercentage: !0,
					roundingValue: 0,
					roundingPercentage: 0
				},
				title: E
			}
		}
	}, U = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		transitions: k,
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		customPalette: [],
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0,
			zoom: !0
		}),
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				height: 500,
				width: 800,
				padding: M([
					0,
					0,
					0,
					0
				]),
				layout: {
					sorted: !0,
					rects: {
						stroke: g,
						strokeWidth: 1,
						borderRadius: 0,
						colorRatio: .3,
						gradient: {
							show: !0,
							intensity: 30
						},
						group: {
							stroke: g,
							strokeWidth: 1,
							useSeriesBackgroundColor: !1,
							backgroundLighterRatio: .4,
							label: {
								adaptColorToBackground: !1,
								color: m
							}
						},
						selected: {
							stroke: g,
							strokeWidth: 1,
							unselectedOpacity: .6
						}
					},
					labels: {
						showDefaultLabels: !0,
						fontSize: y._24,
						fontSizeZoomFactor: 6,
						minFontSize: y._10,
						hideUnderProportion: 0,
						prefix: "",
						suffix: "",
						rounding: 0,
						formatter: null,
						name: {
							show: !0,
							bold: !0
						},
						value: {
							show: !0,
							bold: !1
						}
					}
				},
				legend: {
					...D,
					roundingValue: 0,
					roundingPercentage: 0,
					showValue: !0,
					showPercentage: !0,
					position: "bottom"
				},
				title: E,
				tooltip: {
					...O,
					roundingValue: 1
				},
				zoom: { show: !1 }
			}
		},
		table: {
			...F,
			useDialog: !1,
			columnNames: {
				series: "Series",
				value: "Value",
				percentage: "Percentage"
			},
			th: N,
			td: {
				...P,
				roundingValue: 0,
				roundingPercentage: 0
			}
		}
	}, W = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		customPalette: [],
		useBlurOnHover: !0,
		useCustomCells: !1,
		useAnimation: !0,
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				layout: {
					labels: {
						dataLabels: {
							...T,
							prefix: "",
							suffix: "",
							formatter: null
						},
						captions: {
							show: !1,
							showSerieName: !1,
							serieNameAbbreviation: !0,
							serieNameMaxAbbreviationSize: 3,
							fontSize: y._12,
							showValue: !0,
							showPercentage: !0,
							roundingValue: 0,
							roundingPercentage: 0,
							offsetX: 0,
							offsetY: 0
						}
					},
					grid: {
						size: 10,
						spaceBetween: 2,
						vertical: !1
					},
					rect: {
						rounded: !0,
						rounding: 2,
						stroke: t,
						strokeWidth: 1,
						useGradient: !0,
						gradientIntensity: 40
					}
				},
				title: E,
				tooltip: {
					...O,
					...T,
					showValue: !0,
					showPercentage: !0,
					roundingValue: 0,
					roundingPercentage: 0
				},
				legend: {
					...D,
					...T,
					roundingValue: 0,
					roundingPercentage: 0,
					showValue: !0,
					showPercentage: !0,
					position: "bottom"
				}
			}
		},
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		table: {
			...F,
			useDialog: !1,
			columnNames: {
				series: "Series",
				value: "Value",
				percentage: "Percentage"
			},
			th: N,
			td: {
				...P,
				roundingValue: 0,
				roundingPercentage: 0
			}
		}
	}, G = {
		devHints: w,
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		customPalette: [],
		useCssAnimation: !1,
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				layout: {
					scaleToAxisMax: !1,
					targetReference: {
						show: !1,
						stroke: t,
						strokeWidth: 1,
						strokeDasharray: 4,
						showInLegend: !0,
						legendLabel: "Target"
					},
					plots: {
						show: !0,
						radius: 2
					},
					outerPolygon: {
						stroke: i,
						strokeWidth: 1
					},
					dataPolygon: {
						strokeWidth: 1,
						transparent: !1,
						opacity: 20,
						useGradient: !0
					},
					grid: {
						show: !0,
						stroke: g,
						strokeWidth: .5,
						graduations: 5
					},
					labels: { dataLabels: {
						show: !0,
						fontSize: y._12,
						color: m
					} }
				},
				title: E,
				tooltip: {
					...O,
					showValue: !0,
					showPercentage: !0,
					roundingValue: 0,
					roundingPercentage: 0,
					animation: {
						show: !0,
						animationFrames: 60
					}
				},
				legend: {
					...D,
					roundingPercentage: 0,
					position: "bottom"
				}
			}
		},
		table: {
			...F,
			useDialog: !1,
			th: N,
			td: {
				...P,
				roundingValue: 0,
				roundingPercentage: 0
			}
		},
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		translations: {
			target: "Target",
			value: "Value",
			datapoint: "Datapoint"
		}
	}, K = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		customPalette: [],
		useCssAnimation: !1,
		zoomAnimationFrames: 20,
		zoomEnabled: !0,
		downsample: C,
		style: {
			fontFamily: "inherit",
			chart: {
				height: 512,
				width: 512,
				backgroundColor: p,
				color: m,
				layout: {
					labels: {
						quadrantLabels: {
							show: !0,
							tl: {
								text: "",
								color: m,
								fontSize: y._16,
								bold: !0
							},
							tr: {
								text: "",
								color: m,
								fontSize: y._16,
								bold: !0
							},
							br: {
								text: "",
								color: m,
								fontSize: y._16,
								bold: !0
							},
							bl: {
								text: "",
								color: m,
								fontSize: y._16,
								bold: !0
							}
						},
						plotLabels: {
							showAsTag: !1,
							show: !0,
							fontSize: y._10,
							color: m,
							offsetY: 8,
							rounding: 0,
							x: { formatter: null },
							y: { formatter: null }
						},
						axisLabels: {
							show: !0,
							fontSize: y._14,
							color: {
								positive: m,
								negative: m
							}
						}
					},
					grid: {
						stroke: g,
						strokeWidth: 1.5,
						showArrows: !0,
						graduations: {
							stroke: g,
							strokeWidth: .5,
							show: !0,
							steps: 5,
							fill: !0,
							color: g,
							roundingForce: 10
						},
						xAxis: {
							min: -100,
							max: 100,
							auto: !0,
							name: "",
							show: !0
						},
						yAxis: {
							min: -100,
							max: 100,
							auto: !0,
							name: "",
							show: !0
						}
					},
					plots: {
						radius: 6,
						outline: !0,
						outlineColor: o,
						outlineWidth: 1
					},
					areas: {
						show: !0,
						opacity: 40,
						useGradient: !0
					}
				},
				title: E,
				tooltip: {
					...O,
					roundingValue: 0,
					showShape: !0
				},
				legend: {
					...D,
					position: "bottom"
				}
			}
		},
		table: {
			...F,
			useDialog: !1,
			th: N,
			td: {
				...P,
				roundingValue: 0
			}
		},
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			labels: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		translations: {
			category: "Category",
			item: "Item",
			side: "Side"
		}
	}, q = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		theme: "",
		customPalette: [],
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				animation: {
					use: !0,
					speed: 1,
					acceleration: 1
				},
				layout: {
					radiusRatio: 1,
					track: {
						size: 1,
						useGradient: !0,
						gradientIntensity: 20
					},
					markers: {
						show: !0,
						color: m,
						bold: !0,
						fontSizeRatio: 1,
						offsetY: 0,
						roundingValue: 0,
						formatter: null,
						prefix: "",
						suffix: ""
					},
					segmentSeparators: {
						show: !1,
						offsetOut: 0,
						offsetIn: 0,
						stroke: t,
						strokeWidth: 2
					},
					segmentNames: {
						show: !0,
						curved: !0,
						offsetRatio: 1.1,
						fontSize: y._16,
						minFontSize: b,
						useSerieColor: !0,
						color: m,
						bold: !1
					},
					indicatorArc: {
						show: !1,
						radius: 123,
						fill: g
					},
					pointer: {
						show: !0,
						type: "pointy",
						size: 1,
						stroke: o,
						strokeWidth: 12,
						useRatingColor: !0,
						color: i,
						circle: {
							radius: 10,
							stroke: t,
							strokeWidth: 2,
							color: o
						}
					}
				},
				legend: {
					show: !0,
					fontSize: y._48,
					prefix: "",
					suffix: "",
					roundingValue: 1,
					showPlusSymbol: !0,
					useRatingColor: !0,
					color: m,
					formatter: null
				},
				title: E
			}
		},
		userOptions: j({
			pdf: !0,
			img: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		translations: { base: "Base" }
	}, J = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		theme: "",
		layout: "classic",
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				animation: {
					use: !0,
					speed: .5,
					acceleration: 1
				},
				layout: {
					wheel: {
						radiusRatio: 1,
						tiltAngle3d: 50,
						ticks: {
							type: "classic",
							rounded: !0,
							inactiveColor: g,
							activeColor: c,
							sizeRatio: .9,
							quantity: 100,
							strokeWidth: 5,
							stroke: "transparent",
							spacingRatio3d: 1,
							shadeColorRatio3d: .15,
							depth3d: 0,
							gradient: {
								show: !0,
								shiftHueIntensity: 100
							}
						}
					},
					innerCircle: {
						show: !0,
						stroke: g,
						strokeWidth: 1,
						radiusRatio: 1
					},
					percentage: {
						show: !0,
						fontSize: y._48,
						rounding: 1,
						bold: !0,
						formatter: null,
						offsetX: 0,
						offsetY: 0,
						stroke: "transparent",
						strokeWidth: 0
					}
				},
				title: E
			}
		},
		userOptions: j({
			pdf: !0,
			img: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		})
	}, Y = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		theme: "",
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				width: 312,
				height: 56,
				animation: {
					use: !0,
					speed: .5,
					acceleration: 1
				},
				layout: {
					display: "horizontal",
					crescendo: !1,
					curved: !1,
					curveAngleX: 10,
					curveAngleY: 10,
					activeColor: c,
					inactiveColor: g,
					ticks: { gradient: {
						show: !0,
						shiftHueIntensity: 100
					} }
				},
				percentage: {
					show: !0,
					useGradientColor: !0,
					color: c,
					fontSize: y._16,
					bold: !0,
					rounding: 1,
					verticalPosition: S.BOTTOM,
					horizontalPosition: S.LEFT,
					formatter: null
				},
				title: E
			}
		},
		userOptions: j({
			pdf: !0,
			img: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		})
	}, X = {
		devHints: w,
		debug: !1,
		loading: !1,
		theme: "",
		customPalette: [],
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				layout: {
					grandTotal: {
						show: !0,
						fontSize: y._20,
						bold: !0,
						prefix: "",
						suffix: "",
						roundingValue: 0,
						color: m,
						text: "Grand total",
						offsetY: 0,
						formatter: null
					},
					roots: {
						stroke: o,
						strokeWidth: 5,
						useGradient: !0,
						gradientIntensity: 20,
						underlayerColor: o,
						labels: {
							show: !0,
							fontSize: y._16,
							adaptColorToBackground: !0,
							color: o,
							bold: !0,
							roundingValue: 0,
							prefix: "",
							suffix: "",
							formatter: null,
							name: {
								color: m,
								fontSize: y._16,
								bold: !0
							}
						}
					},
					verticalSeparator: {
						stroke: o,
						strokeWidth: 5
					},
					links: { opacity: 10 },
					branches: {
						stroke: o,
						strokeWidth: 0,
						borderRadius: 6,
						useGradient: !0,
						gradientIntensity: 20,
						underlayerColor: o,
						widthRatio: 1.5,
						labels: {
							show: !0,
							fontSize: y._14,
							color: m,
							bold: !0,
							dataLabels: {
								show: !0,
								hideUnderValue: 5,
								fontSize: y._14,
								roundingValue: 0,
								roundingPercentage: 0,
								prefix: "",
								suffix: "",
								formatter: null
							}
						}
					},
					nuts: {
						offsetX: 20,
						useGradient: !0,
						gradientIntensity: 30,
						selected: {
							useMotion: !0,
							useGradient: !0,
							gradientIntensity: 40,
							roundingValue: 0,
							roundingPercentage: 0,
							labels: {
								dataLabels: {
									hideUnderValue: 5,
									color: m,
									fontSize: y._12,
									bold: !0,
									prefix: "",
									suffix: "",
									formatter: null
								},
								core: {
									total: {
										color: m,
										fontSize: y._24,
										bold: !1
									},
									value: {
										color: m,
										fontSize: y._24,
										bold: !0,
										prefix: "",
										suffix: ""
									}
								}
							}
						}
					},
					legend: {
						fontSize: y._16,
						color: m,
						roundingValue: 0,
						roundingPercentage: 0,
						prefix: "",
						suffix: ""
					},
					title: {
						...E,
						offsetY: 0,
						subtitle: {
							...E.subtitle,
							offsetY: 0
						}
					}
				}
			}
		},
		table: {
			...F,
			useDialog: !1,
			th: {
				...N,
				translations: {
					rootName: "root name",
					rootValue: "root value",
					rootToTotal: "%/total",
					branchName: "branch name",
					branchValue: "branch value",
					branchToRoot: "%/root",
					branchToTotal: "%/total",
					nutName: "nut name",
					nutValue: "nut value",
					nutToBranch: "%/branch",
					nutToRoot: "%/root",
					nutToTotal: "%/total"
				}
			},
			td: {
				...P,
				roundingValue: 0,
				roundingPercentage: 0
			}
		},
		userOptions: j({
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		translations: {
			total: "Total",
			proportionToTree: "of grand total",
			of: "of"
		}
	}, Z = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		transitions: k,
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		customPalette: [],
		useCssAnimation: !1,
		useStartAnimation: !0,
		useBlurOnHover: !0,
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				useGradient: !0,
				gradientIntensity: 20,
				layout: {
					maxThickness: 64,
					gutter: {
						color: g,
						width: .62
					},
					track: { width: .62 },
					labels: {
						...T,
						show: !0,
						fontSize: y._14,
						minFontSize: b,
						color: m,
						roundingValue: 0,
						roundingPercentage: 0,
						bold: !0,
						offsetX: 0,
						offsetY: 0,
						value: {
							show: !0,
							formatter: null
						},
						percentage: { show: !0 }
					}
				},
				title: E,
				legend: {
					...D,
					...T,
					roundingValue: 0,
					roundingPercentage: 0,
					showValue: !0,
					showPercentage: !0,
					position: "bottom"
				},
				tooltip: {
					...O,
					...T,
					showValue: !0,
					showPercentage: !0,
					roundingValue: 0,
					roundingPercentage: 0
				}
			}
		},
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		table: {
			...F,
			useDialog: !1,
			th: N,
			td: {
				...P,
				roundingValue: 0,
				roundingPercentage: 0
			},
			translations: {
				value: "Value",
				percentage: "Percentage",
				serie: "Serie"
			}
		}
	}, Q = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		debug: !1,
		loading: !1,
		autoSize: !0,
		responsive: !1,
		theme: "",
		customPalette: [],
		useCssAnimation: !1,
		transitions: k,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				width: 512,
				height: 316,
				layout: {
					bars: {
						rowColor: null,
						rowRadius: 4,
						sort: ee.DESC,
						useStroke: !1,
						strokeWidth: 2,
						height: 32,
						gap: 6,
						borderRadius: 4,
						offsetX: 12,
						paddingRight: 0,
						useGradient: !0,
						gradientIntensity: 20,
						fillOpacity: 90,
						underlayerColor: o,
						dataLabels: {
							color: m,
							bold: !0,
							fontSize: y._14,
							value: {
								show: !0,
								roundingValue: 0,
								prefix: "",
								suffix: "",
								formatter: null
							},
							percentage: {
								show: !0,
								roundingPercentage: 0
							},
							offsetX: 0
						},
						nameLabels: {
							show: !0,
							color: m,
							bold: !1,
							fontSize: y._14,
							offsetX: 0
						},
						parentLabels: {
							show: !0,
							color: m,
							bold: !1,
							fontSize: y._14,
							offsetX: 0,
							paddingBottom: 0
						}
					},
					highlighter: {
						color: t,
						opacity: 5
					},
					separators: {
						show: !1,
						color: g,
						strokeWidth: 1,
						fullWidth: !0
					}
				},
				title: E,
				legend: {
					...D,
					position: S.TOP,
					roundingValue: 0,
					roundingPercentage: 0,
					prefix: "",
					suffix: "",
					showValue: !0
				},
				tooltip: {
					...O,
					showValue: !0,
					showPercentage: !0,
					roundingValue: 0,
					roundingPercentage: 0,
					prefix: "",
					suffix: ""
				}
			}
		},
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			sort: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		table: {
			...F,
			useDialog: !1,
			th: N,
			td: {
				...P,
				roundingValue: 0,
				roundingPercentage: 0,
				prefix: "",
				suffix: ""
			}
		},
		translations: {
			parentName: "Serie",
			childName: "Child",
			value: "value",
			percentageToTotal: "%/total",
			percentageToSerie: "%/serie"
		}
	}, ne = Q, re = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		style: {
			fontFamily: "inherit",
			backgroundColor: p,
			color: m,
			layout: {
				height: 300,
				width: 1e3,
				padding: M([
					0,
					0,
					0,
					0
				]),
				crosshairs: {
					show: !1,
					stroke: t,
					strokeWidth: 1,
					strokeDasharray: 0
				},
				cells: {
					rowTotal: {
						value: { show: !1 },
						color: { show: !1 }
					},
					columnTotal: {
						value: {
							show: !1,
							rotation: 0,
							autoRotate: {
								enable: !0,
								angle: -30
							},
							offsetX: 0,
							offsetY: 0
						},
						color: { show: !1 }
					},
					value: {
						show: !1,
						fontSize: y._18,
						bold: !1,
						roundingValue: 0,
						color: m,
						formatter: null
					},
					colors: {
						hot: l,
						cold: c,
						underlayer: o
					},
					spacing: 2,
					selected: {
						border: 4,
						color: t
					}
				},
				dataLabels: {
					prefix: "",
					suffix: "",
					xAxis: {
						show: !0,
						values: [],
						datetimeFormatter: A,
						showOnlyAtModulo: null,
						rotation: 0,
						autoRotate: {
							enable: !0,
							angle: -30
						},
						fontSize: y._10,
						color: m,
						bold: !1,
						offsetX: 0,
						offsetY: 0
					},
					yAxis: {
						show: !0,
						values: [],
						datetimeFormatter: A,
						fontSize: y._10,
						color: m,
						bold: !1,
						offsetX: 0,
						offsetY: 0
					}
				}
			},
			title: E,
			legend: {
				...D,
				fontSize: y._12,
				roundingValue: 0,
				width: 24
			},
			tooltip: {
				...O,
				roundingValue: 0
			}
		},
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		table: {
			...F,
			useDialog: !1,
			colNames: { xAxis: "X" },
			th: N,
			td: {
				...P,
				roundingValue: 0
			}
		}
	}, ie = {
		devHints: w,
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		transitions: k,
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		customPalette: [],
		useCssAnimation: !1,
		downsample: C,
		usePerformanceMode: !1,
		style: {
			fontFamily: "inherit",
			backgroundColor: p,
			color: m,
			layout: {
				height: 316,
				width: 512,
				padding: M([
					0,
					0,
					0,
					0
				]),
				axis: {
					show: !0,
					stroke: g,
					strokeWidth: 1,
					xMin: null,
					xMax: null,
					yMin: null,
					yMax: null
				},
				marginalBars: {
					show: !1,
					size: 40,
					tranches: 20,
					opacity: .6,
					fill: t,
					strokeWidth: 1,
					offset: 20,
					borderRadius: 2,
					useGradient: !0,
					showLines: !1,
					linesStrokeWidth: 1,
					highlighter: {
						show: !0,
						opacity: .1,
						color: t,
						stroke: t,
						strokeWidth: .5,
						strokeDasharray: 2,
						highlightBothAxes: !1
					}
				},
				plots: {
					radius: 2,
					hoverRadiusRatio: 2,
					stroke: o,
					strokeWidth: .3,
					opacity: .6,
					opacityNotSelected: .6,
					name: {
						show: !1,
						fontSize: 12,
						offsetY: 0,
						color: t
					},
					significance: {
						show: !0,
						useDistanceOpacity: !1,
						deviationThreshold: 10,
						opacity: .3
					},
					deviation: {
						translation: "deviation",
						roundingValue: 1
					},
					giftWrap: {
						show: !1,
						strokeWidth: 1,
						strokeDasharray: 0,
						fillOpacity: .2
					},
					selectors: {
						show: !0,
						stroke: t,
						strokeWidth: .7,
						strokeDasharray: 0,
						labels: {
							fontSize: y._12,
							color: m,
							rounding: 2,
							bold: !1,
							showName: !0,
							prefix: "",
							suffix: "",
							x: { formatter: null },
							y: { formatter: null }
						},
						markers: {
							radius: 1.5,
							stroke: o,
							strokeWidth: .5,
							fill: t
						}
					}
				},
				correlation: {
					show: !0,
					strokeDasharray: 2,
					strokeWidth: 1,
					label: {
						show: !0,
						fontSize: y._12,
						color: m,
						bold: !0,
						roundingValue: 2,
						useSerieColor: !0
					}
				},
				dataLabels: {
					reverseAxisLabels: !1,
					xAxis: {
						name: "",
						show: !0,
						fontSize: y._10,
						color: m,
						bold: !1,
						offsetX: 0,
						offsetY: 0,
						roundingValue: 0,
						showValue: !0,
						scales: {
							show: !1,
							steps: 10,
							useNiceScale: !0,
							labels: {
								formatter: null,
								color: t,
								fontSize: 12,
								offsetY: 0
							},
							verticalLines: {
								show: !1,
								stroke: r,
								strokeWidth: 1
							}
						}
					},
					yAxis: {
						name: "",
						show: !0,
						fontSize: y._10,
						color: m,
						bold: !1,
						offsetX: 0,
						offsetY: 0,
						roundingValue: 0,
						showValue: !0,
						scales: {
							show: !1,
							steps: 10,
							useNiceScale: !0,
							labels: {
								formatter: null,
								color: t,
								fontSize: 12,
								offsetX: 0
							},
							horizontalLines: {
								show: !1,
								stroke: r,
								strokeWidth: 1
							}
						}
					}
				}
			},
			title: E,
			legend: {
				...D,
				roundingValue: 0,
				position: "bottom"
			},
			tooltip: {
				...O,
				roundingValue: 2,
				showShape: !0,
				prefix: "",
				suffix: ""
			}
		},
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		table: {
			...F,
			useDialog: !1,
			th: N,
			td: {
				...P,
				roundingValue: 2,
				roundingAverage: 1
			},
			translations: {
				correlationCoefficient: "Correlation Coef.",
				nbrPlots: "Nbr plots",
				average: "Average",
				series: "Series"
			}
		}
	}, ae = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		type: "candlestick",
		debug: !1,
		loading: !1,
		responsive: !1,
		responsiveProportionalSizing: !0,
		a11y: v,
		transitions: k,
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		useCssAnimation: !1,
		style: {
			fontFamily: "inherit",
			backgroundColor: p,
			color: m,
			height: 316,
			width: 512,
			layout: {
				padding: M([
					0,
					0,
					0,
					0
				]),
				selector: {
					color: g,
					opacity: 10
				},
				grid: {
					show: !0,
					stroke: g,
					strokeWidth: .5,
					verticalLines: {
						show: !1,
						strokeDasharray: 0,
						strokeWidth: .5,
						stroke: g
					},
					horizontalLines: {
						show: !1,
						strokeDasharray: 0,
						strokeWidth: .5,
						stroke: g
					},
					xAxis: {
						ticks: { show: !0 },
						dataLabels: {
							show: !0,
							fontSize: y._10,
							color: m,
							offsetY: 0,
							bold: !1,
							rotation: 0,
							autoRotate: {
								enable: !0,
								angle: -30
							},
							datetimeFormatter: A,
							showOnlyFirstAndLast: !1,
							showOnlyAtModulo: !1,
							modulo: 12
						}
					},
					yAxis: {
						position: "left",
						scale: {
							min: null,
							max: null
						},
						dataLabels: {
							show: !0,
							fontSize: y._12,
							color: m,
							roundingValue: 0,
							offsetX: 0,
							bold: !1,
							steps: 10,
							prefix: "",
							suffix: ""
						}
					}
				},
				wick: {
					stroke: t,
					strokeWidth: .5,
					extremity: {
						shape: x.LINE,
						size: "auto",
						color: t
					}
				},
				candle: {
					borderRadius: 1,
					stroke: t,
					strokeWidth: .5,
					colors: {
						bearish: l,
						bullish: u
					},
					gradient: {
						show: !0,
						underlayer: o
					},
					widthRatio: .5
				}
			},
			zoom: {
				...I,
				minimap: L,
				preview: {
					enable: !0,
					fill: "#CCCCCC50",
					stroke: "#6A6A6A",
					strokeWidth: 2,
					strokeDasharray: 0
				},
				useDefaultFormat: !0,
				timeFormat: "yyyy-MM-dd HH:mm:ss",
				customFormat: null
			},
			title: E,
			tooltip: {
				...O,
				roundingValue: 0,
				prefix: "",
				suffix: "",
				showChart: !0,
				useDefaultTimeFormat: !0,
				timeFormat: "yyyy-MM-dd HH:mm:ss"
			}
		},
		translations: {
			period: "Period",
			open: "Open",
			high: "High",
			low: "Low",
			last: "Close",
			volume: "Volume"
		},
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		table: {
			...F,
			th: N,
			useDialog: !1,
			td: {
				...P,
				roundingValue: 2,
				prefix: "",
				suffix: ""
			}
		}
	}, oe = {
		devHints: w,
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		loading: !1,
		theme: "",
		responsive: !1,
		type: x.LINE,
		downsample: C,
		a11y: v,
		translations: {
			period: "period",
			value: "value"
		},
		gradientPath: {
			show: !1,
			segments: 256,
			colors: {
				high: "#34eb96",
				low: "#eb4034"
			}
		},
		temperatureColors: {
			show: !1,
			colors: ["#34eb96", "#eb4034"]
		},
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		style: {
			chartWidth: 290,
			animation: {
				show: !0,
				animationFrames: 360
			},
			padding: M([
				12,
				12,
				3,
				0
			]),
			fontFamily: "inherit",
			backgroundColor: p,
			scaleMin: null,
			scaleMax: null,
			line: {
				color: c,
				strokeWidth: 3,
				smooth: !1,
				cutNullValues: !1,
				dashIndices: [],
				dashArray: 4,
				pulse: {
					show: !1,
					loop: !0,
					color: o,
					durationMs: 4e3,
					easing: "ease-in-out",
					radius: 2,
					cubicBezier: [
						.45,
						.45,
						.55,
						.55
					],
					trail: {
						show: !1,
						length: 6,
						opacity: .75
					}
				}
			},
			bar: {
				borderRadius: 3,
				color: c
			},
			zeroLine: {
				color: t,
				strokeWidth: 1
			},
			plot: {
				show: !0,
				radius: 4,
				stroke: o,
				strokeWidth: 1
			},
			verticalIndicator: {
				show: !0,
				strokeWidth: 1.5,
				color: c,
				strokeDasharray: 3
			},
			dataLabel: {
				show: !0,
				position: S.LEFT,
				offsetX: 0,
				offsetY: 0,
				fontSize: y._20,
				bold: !0,
				color: m,
				roundingValue: 0,
				valueType: "latest",
				prefix: "",
				suffix: "",
				formatter: null,
				datetimeFormatter: A
			},
			title: {
				show: !0,
				textAlign: S.LEFT,
				color: m,
				fontSize: y._16,
				bold: !0,
				text: ""
			},
			tooltip: {
				show: !1,
				fontSize: y._14,
				color: m,
				backgroundColor: p,
				offsetY: 0,
				borderWidth: 0,
				borderColor: _,
				borderRadius: 2,
				backgroundOpacity: 100
			},
			area: {
				show: !0,
				useGradient: !0,
				opacity: 30,
				color: c
			}
		}
	}, se = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		debug: !1,
		loading: !1,
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		customPalette: [],
		style: {
			fontFamily: "inherit",
			backgroundColor: p,
			animation: {
				show: !0,
				animationFrames: 60
			},
			layout: {
				independant: !0,
				percentage: !0,
				target: 0,
				showTargetValue: !1,
				targetValueText: ""
			},
			gutter: {
				backgroundColor: g,
				opacity: 100
			},
			bar: { gradient: {
				show: !0,
				intensity: 40,
				underlayerColor: o
			} },
			labels: {
				fontSize: y._16,
				name: {
					position: "top-left",
					width: "100%",
					color: m,
					bold: !1
				},
				value: {
					show: !0,
					bold: !0
				}
			},
			title: {
				text: "",
				color: m,
				fontSize: y._16,
				bold: !0,
				textAlign: S.LEFT,
				margin: "0 0 6px 0",
				subtitle: {
					color: h,
					text: "",
					fontSize: y._12,
					bold: !1
				}
			},
			gap: 4
		}
	}, ce = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		debug: !1,
		loading: !1,
		useCursorPointer: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				series: "Series",
				percentage: "Percentage",
				value: "Value"
			}
		},
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		customPalette: [],
		style: {
			fontFamily: "inherit",
			backgroundColor: p,
			animation: {
				show: !0,
				animationFrames: 60
			},
			bar: { gradient: {
				show: !0,
				intensity: 40,
				underlayerColor: o
			} },
			legend: {
				show: !0,
				textAlign: S.LEFT,
				fontSize: y._12,
				margin: "6px 0 0 0",
				name: {
					color: m,
					bold: !1
				},
				value: {
					show: !0,
					bold: !1,
					color: m,
					prefix: "",
					suffix: "",
					rounding: 0,
					formatter: null
				},
				percentage: {
					show: !0,
					bold: !0,
					color: m,
					rounding: 1
				},
				selectAllToggle: {
					show: !1,
					backgroundColor: _,
					color: m
				}
			},
			title: {
				...E,
				textAlign: S.LEFT,
				margin: "0 0 6px 0"
			},
			tooltip: { ...O }
		}
	}, le = {
		devHints: w,
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				series: "Series",
				time: "Time",
				value: "Value",
				label: "Label"
			}
		},
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		style: {
			fontFamily: "inherit",
			backgroundColor: p,
			animation: {
				show: !0,
				speedMs: 500
			},
			layout: {
				height: 96,
				width: 640,
				padding: M([
					24,
					0,
					36,
					0
				])
			},
			bars: {
				shape: x.SQUARE,
				strokeWidth: 0,
				colors: {
					positive: c,
					negative: l,
					gradient: { show: !0 }
				},
				borderRadius: 24,
				gap: 12
			},
			labels: {
				value: {
					show: !0,
					fontSize: y._14,
					minFontSize: b,
					color: m,
					bold: !0,
					rounding: 1,
					prefix: "",
					suffix: "",
					offsetY: 0,
					formatter: null
				},
				valueLabel: {
					show: !0,
					fontSize: y._14,
					minFontSize: b,
					color: m,
					bold: !1,
					rounding: 0
				},
				timeLabel: {
					show: !0,
					fontSize: y._12,
					minFontSize: b,
					color: m,
					bold: !1
				}
			},
			selector: {
				stroke: c,
				fill: "#2D353C10",
				strokeWidth: 2,
				strokeDasharray: 0,
				borderRadius: 2
			},
			title: {
				text: "",
				color: m,
				fontSize: y._16,
				bold: !0,
				textAlign: S.LEFT,
				subtitle: {
					color: h,
					text: "",
					fontSize: y._12,
					bold: !1
				},
				margin: "0 0 6px 0"
			}
		}
	}, ue = {
		devHints: w,
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		loading: !1,
		theme: "",
		style: {
			fontFamily: "inherit",
			background: o,
			height: 84,
			basePosition: 72,
			animation: {
				show: !0,
				speedMs: 150
			},
			title: {
				show: !0,
				fontSize: y._12,
				position: S.TOP,
				textAlign: S.CENTER,
				bold: !1,
				color: m
			},
			dataLabel: {
				fontSize: y._20,
				autoColor: !0,
				color: m,
				offsetY: 0,
				bold: !0,
				rounding: 0,
				prefix: "",
				suffix: "",
				formatter: null
			},
			colors: {
				min: l,
				max: u,
				showGradient: !0
			},
			track: {
				autoColor: !0,
				color: c,
				strokeLinecap: x.ROUND
			},
			gutter: {
				color: g,
				strokeLinecap: x.ROUND
			}
		}
	}, de = {
		devHints: w,
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		theme: "",
		downsample: C,
		style: {
			fontFamily: "inherit",
			backgroundColor: p,
			height: 80,
			width: 300,
			animation: {
				show: !0,
				animationFrames: 20
			},
			line: {
				stroke: t,
				strokeWidth: 2,
				strokeLinecap: x.ROUND,
				strokeLinejoin: x.ROUND,
				smooth: !0,
				useColorTrend: !0
			},
			area: {
				show: !0,
				useGradient: !0,
				opacity: 20
			},
			dataLabel: {
				show: !0,
				useColorTrend: !0,
				color: m,
				fontSize: y._14,
				bold: !1,
				prefix: "",
				suffix: "",
				rounding: 0,
				formatter: null
			},
			trendLabel: {
				trendType: "global",
				useColorTrend: !0,
				color: m,
				fontSize: y._14,
				bold: !0,
				rounding: 0
			},
			arrow: { colors: {
				positive: u,
				neutral: n,
				negative: l
			} },
			padding: M([
				12,
				12,
				12,
				82
			])
		}
	}, fe = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		debug: !1,
		loading: !1,
		a11y: v,
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		transitions: k,
		responsive: !1,
		theme: "",
		axisLabelsFontSize: y._12,
		backgroundColor: p,
		barGap: 12,
		barAnimated: !0,
		barStrokeWidth: 1,
		blurOnHover: !0,
		chartIsBarUnderDatasetLength: 6,
		color: m,
		customPalette: [],
		dataLabelFontSize: y._14,
		dataLabelRoundingPercentage: 1,
		dataLabelRoundingValue: 0,
		donutHideLabelUnderPercentage: 3,
		donutCurvedMarkers: !0,
		donutLabelMarkerStrokeWidth: 1,
		donutRadiusRatio: .4,
		donutShowTotal: !0,
		donutStrokeWidth: 2,
		donutStroke: p,
		donutThicknessRatio: .18,
		donutTotalLabelFontSize: y._24,
		donutTotalLabelOffsetY: 0,
		donutTotalLabelText: "Total",
		donutUseShadow: !1,
		donutShadowColor: t,
		fontFamily: "inherit",
		formatter: null,
		height: 338,
		legendFontSize: y._12,
		legendIcon: "circleFill",
		legendIconSize: y._12,
		legendPosition: "bottom",
		showLegendSelectAllToggle: !1,
		legendSelectAllToggleBackgroundColor: _,
		legendSelectAllToggleColor: m,
		lineAnimated: !0,
		lineSmooth: !0,
		lineStrokeWidth: 2,
		paletteStartIndex: 0,
		showDataLabels: !0,
		showLegend: !0,
		showTooltip: !0,
		showUserOptions: !0,
		userOptionsPosition: "right",
		showUserOptionsOnChartHover: !1,
		keepUserOptionsStateOnChartLeave: !0,
		useCursorPointer: !1,
		userOptionsButtons: {
			tooltip: !0,
			pdf: !0,
			img: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0,
			altCopy: !1
		},
		userOptionsButtonTitles: {
			open: "Open options",
			close: "Close options",
			tooltip: "Toggle tooltip",
			pdf: "Download PDF",
			img: "Download PNG",
			fullscreen: "Toggle fullscreen",
			annotator: "Toggle annotator",
			svg: "Download SVG",
			altCopy: "Copy alt text"
		},
		userOptionsPrint: {
			overflowTolerance: .2,
			orientation: "auto",
			scale: 2
		},
		userOptionsCallbacks: {
			tooltip: null,
			pdf: null,
			img: null,
			fullscreen: null,
			annotator: null,
			svg: null,
			altCopy: null
		},
		title: "",
		titleBold: !0,
		titleFontSize: y._16,
		titleTextAlign: "center",
		tooltipTeleportTo: "body",
		tooltipCustomFormat: null,
		tooltipBackgroundOpacity: 100,
		tooltipBorderRadius: 4,
		tooltipBorderColor: r,
		tooltipBorderWidth: 1,
		tooltipFontSize: y._14,
		tooltipPosition: S.CENTER,
		tooltipOffsetX: 0,
		tooltipOffsetY: 24,
		tooltipSmooth: !0,
		tooltipSmoothForce: .18,
		tooltipSmoothSnapThreshold: .25,
		tooltipBackdropFilter: !0,
		useCustomLegend: !1,
		valuePrefix: "",
		valueSuffix: "",
		width: 512,
		xAxisLabel: "",
		xyAxisStroke: g,
		xyAxisStrokeWidth: 1,
		xyGridStroke: g,
		xyGridStrokeWidth: .5,
		xyHighlighterColor: t,
		xyHighlighterOpacity: .05,
		xyLabelsXFontSize: y._10,
		xyLabelsYFontSize: y._12,
		xyPaddingBottom: 12,
		xyPaddingLeft: 12,
		xyPaddingRight: 12,
		xyPaddingTop: 12,
		xyPeriodLabelsRotation: 0,
		xyPeriodLabelsAutoRotate: {
			enable: !0,
			angle: -30
		},
		xyPeriods: [],
		datetimeFormatter: A,
		xyPeriodsShowOnlyAtModulo: !1,
		xyPeriodsModulo: 12,
		xyScaleSegments: 15,
		xyShowAxis: !0,
		xyShowGrid: !0,
		xyShowScale: !0,
		yAxisLabel: "",
		zoomXy: !0,
		zoomXyAutoFit: !1,
		zoomColor: i,
		zoomHighlightColor: a,
		zoomFontSize: y._14,
		zoomUseResetSlot: !1,
		zoomMinimap: R,
		zoomStartIndex: null,
		zoomEndIndex: null,
		zoomEnableRangeHandles: !0,
		zoomEnableSelectionDrag: !0,
		zoomFocusOnDrag: !1,
		zoomFocusRangeRatio: .2,
		zoomMaxWidth: null
	}, pe = {
		devHints: w,
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		style: {
			fontFamily: "inherit",
			backgroundColor: p,
			color: m,
			height: 500,
			width: 500,
			layout: {
				padding: M([
					12,
					12,
					36,
					12
				]),
				grid: {
					show: !0,
					stroke: g,
					strokeWidth: 1
				},
				dataLabels: {
					sideTitles: {
						show: !0,
						fontSize: y._18,
						color: m,
						useSideColor: !0,
						bold: !1,
						offsetY: 0
					},
					xAxis: {
						show: !0,
						fontSize: y._12,
						color: m,
						bold: !1,
						scale: 1e3,
						translation: "in thousands",
						formatter: null,
						rotation: 0,
						autoRotate: {
							enable: !0,
							angle: -30
						}
					},
					yAxis: {
						show: !0,
						display: "age",
						fontSize: y._12,
						color: m,
						bold: !1,
						showEvery: 5,
						formatter: null
					}
				},
				centerSlit: { width: 20 },
				bars: {
					gap: 2,
					borderRadius: 2,
					left: { color: l },
					right: { color: c },
					gradient: {
						show: !0,
						underlayer: o,
						intensity: 60,
						shiftHue: .05
					}
				}
			},
			highlighter: {
				color: t,
				opacity: 5
			},
			title: E,
			tooltip: {
				...O,
				roundingValue: 0
			}
		},
		translations: {
			age: "age",
			male: "male",
			female: "female",
			total: "total",
			year: "year"
		},
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		table: {
			...F,
			useDialog: !1,
			th: N,
			td: P
		}
	}, me = {
		devHints: w,
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		responsiveProportionalSizing: !0,
		theme: "",
		customPalette: [],
		style: {
			fontFamily: "inherit",
			backgroundColor: p,
			color: m,
			size: 400,
			limit: 50,
			animation: {
				show: !0,
				speedMs: 300
			},
			labels: {
				color: m,
				fontSize: y._14,
				minFontSize: b
			},
			weightLabels: {
				size: 8,
				show: !0,
				formatter: null,
				prefix: "",
				suffix: "",
				rounding: 0
			},
			links: {
				curved: !1,
				maxWidth: 3
			},
			circle: {
				radiusProportion: .2,
				stroke: i,
				strokeWidth: 1,
				offsetY: 0
			},
			plot: {
				radius: 3,
				useSerieColor: !0,
				color: t
			},
			title: E
		},
		userOptions: j({
			pdf: !0,
			img: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		})
	}, he = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		theme: "",
		customPalette: [],
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				height: 360,
				width: 256,
				thermometer: { width: 48 },
				padding: {
					top: 12,
					bottom: 12
				},
				graduations: {
					show: !0,
					sides: "both",
					height: 2,
					stroke: g,
					strokeWidth: 1,
					showIntermediate: !0,
					gradient: {
						show: !0,
						intensity: 20
					}
				},
				animation: {
					use: !0,
					speedMs: 1e3
				},
				label: {
					show: !0,
					fontSize: y._20,
					minFontSize: b,
					rounding: 1,
					bold: !0,
					color: m,
					prefix: "",
					suffix: "",
					formatter: null
				}
			},
			title: E
		},
		userOptions: j({
			pdf: !0,
			img: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		})
	}, ge = {
		devHints: w,
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		transitions: k,
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		customPalette: [],
		useCssAnimation: !1,
		useBlurOnHover: !0,
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				size: 360,
				layout: {
					labels: { dataLabels: {
						...T,
						prefix: "",
						suffix: "",
						formatter: null,
						show: !1,
						offsetX: 0,
						fontSize: 10,
						color: m,
						bold: !1,
						roundingValue: 0,
						roundingPercentage: 0,
						showValue: !0,
						showPercentage: !0,
						markers: {
							stroke: "#8A8A8A",
							strokeWidth: 1,
							radius: 3,
							position: "right"
						}
					} },
					rings: {
						strokeWidth: 2,
						stroke: g,
						gradient: {
							show: !0,
							intensity: 40,
							underlayerColor: o
						},
						useShadow: !0
					}
				},
				legend: {
					...D,
					roundingValue: 0,
					roundingPercentage: 0,
					showValue: !0,
					showPercentage: !0,
					position: "bottom"
				},
				title: E,
				tooltip: {
					...O,
					showValue: !0,
					showPercentage: !0,
					roundingValue: 0,
					roundingPercentage: 0
				}
			}
		},
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0,
			labels: !0
		}),
		table: {
			...F,
			useDialog: !1,
			columnNames: {
				series: "Series",
				value: "Value",
				percentage: "Percentage"
			},
			th: N,
			td: {
				...P,
				roundingValue: 0,
				roundingPercentage: 0
			}
		}
	}, _e = {
		devHints: w,
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		theme: "",
		a11y: v,
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		transitions: k,
		customPalette: [],
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				zoom: I,
				donuts: { hover: { hideLabelsUnderValue: 5 } },
				dialog: {
					show: !0,
					backgroundColor: p,
					color: m,
					header: {
						backgroundColor: p,
						color: m
					},
					donutChart: {
						...H,
						responsive: !0,
						userOptions: {
							...H.userOptions,
							buttons: {
								...H.userOptions.buttons,
								pdf: !1
							}
						},
						style: {
							...H.style,
							chart: {
								...H.style.chart,
								layout: {
									...H.style.chart.layout,
									donut: {
										...H.style.chart.layout.donut,
										strokeWidth: 64
									},
									labels: {
										...H.style.chart.layout.labels,
										percentage: {
											...H.style.chart.layout.labels.percentage,
											fontSize: 16
										},
										name: {
											...H.style.chart.layout.labels.name,
											fontSize: 12
										},
										hollow: {
											...H.style.chart.layout.labels.hollow,
											average: {
												...H.style.chart.layout.labels.hollow.average,
												show: !1
											}
										}
									}
								}
							}
						}
					}
				},
				layout: {
					height: 316,
					width: 512,
					padding: M([
						5,
						10,
						5,
						10
					]),
					grid: {
						show: !0,
						stroke: g,
						strokeWidth: .7,
						showVerticalLines: !0,
						axis: {
							yLabel: "",
							yLabelOffsetX: 0,
							xLabel: "",
							xLabelOffsetY: 0,
							fontSize: y._14,
							color: m
						},
						yAxis: {
							position: "left",
							scaleMin: null,
							scaleMax: null,
							autoScale: !1,
							dataLabels: {
								show: !0,
								fontSize: y._10,
								color: m,
								roundingValue: 0,
								offsetX: 0,
								bold: !1,
								steps: 10
							}
						},
						xAxis: { dataLabels: {
							show: !0,
							values: [],
							datetimeFormatter: A,
							fontSize: y._10,
							showOnlyFirstAndLast: !1,
							color: m,
							rotation: 0,
							autoRotate: {
								enable: !0,
								angle: -30
							},
							offsetY: 0
						} }
					},
					line: {
						show: !0,
						stroke: i,
						strokeWidth: 4
					},
					highlighter: {
						color: t,
						opacity: 5
					},
					dataLabels: {
						show: !0,
						fontSize: y._10,
						color: m,
						bold: !1,
						rounding: 0,
						prefix: "",
						suffix: "",
						offsetY: 0,
						formatter: null
					}
				},
				title: E,
				legend: {
					...D,
					roundingValue: 0,
					roundingPercentage: 0,
					showValue: !0,
					showPercentage: !0,
					position: "bottom"
				}
			}
		},
		userOptions: j({
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		table: {
			...F,
			useDialog: !1,
			columnNames: {
				period: "Period",
				total: "Total"
			},
			th: N,
			td: {
				...P,
				roundingValue: 0,
				roundingPercentage: 0
			}
		}
	}, ve = {
		devHints: w,
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				layout: {
					grid: {
						show: !0,
						stroke: g,
						strokeWidth: .5
					},
					outerPolygon: {
						stroke: g,
						strokeWidth: 1
					},
					dataPolygon: {
						color: c,
						opacity: 60,
						gradient: {
							show: !0,
							intensity: 1
						},
						stroke: c,
						strokeWidth: 0
					},
					smileys: {
						strokeWidth: 1,
						colors: {
							1: "#e20001",
							2: "#ff9f03",
							3: "#ffd004",
							4: "#9ac900",
							5: "#059f00"
						}
					},
					dataLabel: {
						color: m,
						roundingValue: 0,
						roundingPercentage: 0,
						bold: !0,
						prefix: "",
						suffix: "",
						formatter: null
					}
				},
				title: E,
				legend: {
					...D,
					roundingValue: 0,
					roundingPercentage: 0,
					position: "bottom"
				}
			}
		},
		userOptions: j({
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		table: {
			...F,
			useDialog: !1,
			columnNames: {
				series: "Series",
				value: "Value",
				percentage: "Percentage"
			},
			th: N,
			td: {
				...P,
				roundingValue: 0,
				roundingPercentage: 0
			}
		}
	}, ye = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		debug: !1,
		loading: !1,
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		customPalette: [],
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				nodes: {
					stroke: o,
					strokeHovered: t
				},
				links: {
					stroke: i,
					strokeWidth: 2,
					useChildColor: !1
				},
				title: E,
				tooltip: O,
				zoom: {
					show: !0,
					speed: 1
				}
			}
		},
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			labels: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0,
			zoom: !0
		}),
		table: {
			...F,
			useDialog: !1,
			th: N,
			td: P,
			translations: {
				nodeName: "Node name",
				details: "Details",
				ancestor: "Parent node"
			}
		}
	}, be = {
		devHints: w,
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		theme: "",
		customPalette: [],
		useCssAnimation: !1,
		useBlurOnHover: !0,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		serieToggleAnimation: {
			show: !0,
			durationMs: 500
		},
		startAnimation: {
			show: !1,
			durationMs: 1e3,
			staggerMs: 50
		},
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			labels: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				useGradient: !0,
				gradientIntensity: 40,
				padding: M([
					0,
					0,
					0,
					0
				]),
				width: 512,
				height: 512,
				layout: {
					labels: { dataLabels: {
						...T,
						show: !0,
						prefix: "",
						suffix: "",
						hideUnderValue: 3,
						fontSize: y._14,
						offsetX: 4,
						offsetY: 12,
						useSerieColor: !1,
						color: m,
						showValue: !0,
						showPercentage: !0,
						boldValue: !1,
						boldPercentage: !0,
						roundingValue: 0,
						roundingPercentage: 0,
						showDonutName: !0,
						boldDonutName: !0,
						curvedDonutName: !0,
						donutNameAbbreviation: !1,
						donutNameMaxAbbreviationSize: 3,
						donutNameOffsetY: 0,
						formatter: null
					} },
					donut: {
						strokeWidth: 200,
						borderWidth: 2,
						spacingRatio: .5,
						useShadow: !1,
						shadowColor: t,
						emptyFill: g,
						selectedColor: "#0000001A",
						borderColorAuto: !0,
						borderColor: _
					}
				},
				legend: {
					...D,
					...T,
					roundingValue: 0,
					roundingPercentage: 0,
					showValue: !0,
					showPercentage: !0,
					position: "bottom"
				},
				title: E,
				tooltip: {
					...O,
					...T,
					showAllItemsAtIndex: !0,
					showValue: !0,
					showPercentage: !0,
					roundingValue: 0,
					roundingPercentage: 0
				}
			}
		},
		table: {
			...F,
			useDialog: !1,
			columnNames: {
				series: "Series",
				value: "Value",
				percentage: "Percentage"
			},
			th: N,
			td: {
				...P,
				roundingValue: 0,
				roundingPercentage: 0
			}
		}
	}, xe = {
		devHints: w,
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		transitions: k,
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		customPalette: [],
		useCssAnimation: !1,
		useBlurOnHover: !0,
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				layout: {
					arcs: {
						strokeWidth: 24,
						borderWidth: 12,
						offsetX: 0,
						offsetY: 0,
						hoverEffect: {
							show: !0,
							multiplicator: 1.1
						},
						gradient: {
							show: !0,
							intensity: 30,
							color: o
						}
					},
					labels: { dataLabels: {
						...T,
						prefix: "",
						suffix: "",
						formatter: null
					} }
				},
				legend: {
					...D,
					roundingValue: 0,
					roundingPercentage: 0,
					showValue: !0,
					showPercentage: !0,
					position: "bottom"
				},
				title: E,
				tooltip: {
					...O,
					showValue: !0,
					showPercentage: !0,
					roundingValue: 0,
					roundingPercentage: 0
				}
			}
		},
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		table: {
			...F,
			useDialog: !1,
			columnNames: {
				series: "Series",
				value: "Value",
				percentage: "Percentage"
			},
			th: N,
			td: {
				...P,
				roundingValue: 0,
				roundingPercentage: 0
			}
		}
	}, Se = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		type: 0,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		transitions: k,
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		responsiveProportionalSizing: !0,
		theme: "",
		customPalette: [],
		useCssAnimation: !0,
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			labels: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				height: 600,
				width: 600,
				padding: M([
					12,
					12,
					12,
					12
				]),
				grid: {
					show: !0,
					stroke: i,
					strokeWidth: 1,
					scaleSteps: 10,
					horizontalGrid: {
						show: !0,
						stroke: i,
						strokeWidth: .5,
						strokeDasharray: 4
					},
					verticalGrid: {
						show: !0,
						stroke: i,
						strokeWidth: .5,
						strokeDasharray: 4
					}
				},
				plots: {
					opacity: .9,
					radius: 4,
					stroke: o,
					strokeWidth: 1,
					shape: x.CIRCLE,
					gradient: {
						show: !0,
						intensity: 40
					}
				},
				violin: {
					widthRatio: 1,
					strokeWidth: 1,
					opacity: .2,
					strokeOpacity: .35,
					useSerieColor: !0,
					stroke: "#5A5A5A",
					fill: "#5A5A5A",
					boxPlot: {
						show: !0,
						widthRatio: 1,
						useSerieColor: !0,
						color: "#5A5A5A",
						medianCircleRadiusRatio: 1,
						medianCircleFill: o
					},
					tooltipLabels: {
						upperAdjacent: "Upper adjacent",
						q3: "Q3",
						median: "Median",
						q1: "Q1",
						lowerAdjacent: "Lower adjacent",
						iqr: "IQR",
						count: "Count"
					}
				},
				labels: {
					prefix: "",
					suffix: "",
					formatter: null,
					bestPlotLabel: {
						show: !0,
						showValue: !0,
						fontSize: y._14,
						color: m,
						rounding: 0,
						offsetY: 0
					},
					axis: {
						xLabel: "",
						xLabelOffsetY: 0,
						yLabel: "",
						yLabelOffsetX: 0,
						fontSize: y._14,
						color: m
					},
					xAxisLabels: {
						show: !0,
						color: m,
						fontSize: y._14,
						offsetY: 0,
						rotation: 0,
						autoRotate: {
							enable: !0,
							angle: -30
						}
					},
					yAxisLabels: {
						show: !0,
						color: m,
						fontSize: y._14,
						rounding: 0,
						offsetX: 0
					}
				},
				title: E,
				tooltip: {
					...O,
					roundingValue: 0
				}
			}
		},
		table: {
			...F,
			useDialog: !1,
			columnNames: {
				series: "Series",
				value: "Value"
			},
			th: N,
			td: {
				...P,
				roundingValue: 0
			}
		}
	}, Ce = {
		devHints: w,
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		useAnimation: !1,
		animationSpeed: 2,
		userOptions: j({
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				width: 600,
				rowHeight: 48,
				padding: M([
					12,
					24,
					12,
					12
				]),
				plots: {
					startColor: l,
					endColor: c,
					evaluationColors: {
						enable: !1,
						positive: "#2ca02c",
						negative: "#d62728",
						neutral: "#c7c7c7"
					},
					radius: 6,
					stroke: o,
					strokeWidth: 1,
					link: {
						strokeWidth: 2,
						type: x.CURVED
					},
					gradient: {
						show: !0,
						intensity: 40
					}
				},
				grid: {
					strokeWidth: 1,
					scaleSteps: 10,
					scaleMin: null,
					scaleMax: null,
					horizontalGrid: {
						show: !0,
						stroke: i,
						strokeWidth: .5,
						strokeDasharray: 0
					},
					verticalGrid: {
						show: !0,
						stroke: i,
						strokeWidth: .5,
						strokeDasharray: 0
					}
				},
				comparisonLines: {
					show: !0,
					strokeWidth: 1,
					strokeDasharray: 4,
					showRect: !0,
					rectColor: t,
					rectOpacity: 5,
					showLabel: !0,
					labelColor: t,
					labelFontSize: y._12
				},
				highlighter: {
					color: t,
					opacity: 5
				},
				labels: {
					prefix: "",
					suffix: "",
					formatter: null,
					axis: {
						yLabel: "",
						yLabelOffsetX: 0,
						xLabel: "",
						xLabelOffsetY: 0,
						fontSize: y._14,
						color: m
					},
					yAxisLabels: {
						show: !0,
						fontSize: y._14,
						color: m,
						offsetX: 0,
						bold: !0,
						showProgression: !0,
						rounding: 1,
						formatter: null
					},
					xAxisLabels: {
						show: !0,
						fontSize: y._14,
						color: m,
						offsetY: 0,
						bold: !1,
						rounding: 0,
						rotation: 0,
						autoRotate: {
							enable: !0,
							angle: -30
						}
					},
					startLabels: {
						show: !0,
						fontSize: y._10,
						color: m,
						offsetY: 0,
						rounding: 0,
						useStartColor: !0,
						useEvaluationColor: !0
					},
					endLabels: {
						show: !0,
						fontSize: y._10,
						color: m,
						offsetY: 0,
						rounding: 0,
						useEndColor: !0,
						useEvaluationColor: !0
					}
				},
				legend: {
					...D,
					labelStart: "start",
					labelEnd: "end",
					labelPositive: "positive",
					labelNegative: "negative",
					labelNeutral: "neutral",
					position: "bottom"
				},
				title: E
			}
		},
		table: {
			...F,
			useDialog: !1,
			columnNames: {
				series: "Series",
				start: "Start value",
				end: "End value",
				progression: "Progression"
			},
			th: N,
			td: {
				...P,
				roundingValue: 0,
				roundingPercentage: 0
			}
		}
	}, we = {
		devHints: w,
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		customPalette: [],
		useCssAnimation: !1,
		style: {
			fontFamily: "inherit",
			shape: x.BAR,
			chart: {
				animation: {
					use: !0,
					speed: 1,
					acceleration: 1
				},
				backgroundColor: p,
				color: m,
				bar: {
					color: c,
					stroke: c,
					strokeWidth: .7,
					shadeColor: t
				},
				box: {
					stroke: i,
					strokeWidth: .7,
					strokeDasharray: 2,
					dimensions: {
						width: 128,
						height: 256,
						top: 27,
						bottom: 9,
						left: 24,
						right: 24,
						perspective: 18
					}
				},
				title: E,
				legend: {
					showDefault: !0,
					fontSize: y._10,
					color: m,
					bold: !1,
					roundingValue: 0,
					roundingPercentage: 0,
					prefix: "",
					suffix: "",
					hideUnderPercentage: 3
				},
				dataLabel: {
					show: !0,
					bold: !0,
					color: c,
					fontSize: y._12,
					rounding: 1,
					formatter: null
				}
			}
		},
		userOptions: j({
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		table: {
			...F,
			useDialog: !1,
			columnNames: {
				series: "Series",
				value: "Value",
				percentage: "Percentage"
			},
			th: N,
			td: {
				...P,
				roundingValue: 0,
				roundingPercentage: 0
			}
		}
	}, Te = {
		debug: !1,
		devHints: w,
		theme: "",
		customPalette: [],
		responsiveBreakpoint: 500,
		showAverage: !0,
		showMedian: !0,
		showTotal: !0,
		roundingAverage: 0,
		roundingMedian: 0,
		roundingValues: 0,
		roundingTotal: 0,
		prefix: "",
		suffix: "",
		formatter: null,
		showSparklines: !0,
		fontFamily: "inherit",
		colNames: [],
		sortedDataColumnIndices: [],
		sortedSeriesName: !1,
		sortedSum: !1,
		sortedAverage: !1,
		sortedMedian: !1,
		resetSortOnClickOutside: !1,
		sparkline: {
			useGradient: !0,
			showArea: !0,
			strokeWidth: 3,
			type: x.LINE,
			smooth: !0,
			cutNullValues: !1,
			dimensions: {
				width: 150,
				heightRatio: 1
			},
			animation: {
				show: !0,
				animationFrames: 360
			}
		},
		translations: {
			serie: "Serie",
			total: "Total",
			average: "Average",
			median: "Median",
			chart: "Evolution"
		},
		title: {
			text: "",
			color: m,
			fontSize: y._20,
			bold: !0,
			textAlign: "center",
			backgroundColor: p,
			subtitle: {
				color: h,
				text: "",
				fontSize: y._16,
				bold: !1
			}
		},
		thead: {
			backgroundColor: p,
			color: m,
			fontSize: y._14,
			outline: "none",
			textAlign: S.LEFT,
			bold: !1
		},
		tbody: {
			showColorMarker: !0,
			backgroundColor: p,
			color: m,
			fontSize: y._14,
			outline: "none",
			textAlign: S.LEFT,
			bold: !1,
			selectedColor: {
				useSerieColor: !0,
				fallback: g
			}
		},
		userOptions: j({
			pdf: !0,
			csv: !0,
			img: !0,
			fullscreen: !0
		})
	}, Ee = {
		debug: !1,
		devHints: w,
		theme: "",
		style: {
			fontFamily: "inherit",
			backgroundColor: p,
			color: m,
			shapeSize: y._14,
			heatmapColors: {
				useIndividualScale: !1,
				min: o,
				max: c
			}
		},
		table: {
			responsiveBreakpoint: 400,
			borderWidth: 1,
			showSum: !1,
			showAverage: !1,
			showMedian: !1,
			head: {
				backgroundColor: p,
				color: m,
				values: []
			}
		},
		userOptions: j({
			pdf: !0,
			csv: !0,
			img: !0,
			fullscreen: !0
		})
	}, De = {
		devHints: w,
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		customPalette: [],
		useCssAnimation: !1,
		animationDelayMs: 20,
		quality: "fast",
		strictPixelPadding: !0,
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0,
			zoom: !0
		}),
		nodeCategories: {},
		nodeCategoryColors: {},
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				height: 512,
				width: 512,
				zoom: { show: !0 },
				controls: {
					position: "bottom",
					show: !0,
					backgroundColor: g,
					buttonColor: g,
					color: m,
					fontSize: 14,
					border: `1px solid ${_}`,
					padding: "0.5rem",
					borderRadius: "0.25rem"
				},
				words: {
					maxFontSize: 100,
					minFontSize: y._10,
					bold: !1,
					proximity: 0,
					packingWeight: 1,
					color: m,
					usePalette: !0,
					hoverOpacity: .5,
					selectedStroke: "transparent"
				},
				title: E,
				tooltip: {
					...O,
					roundingValue: 0
				}
			}
		},
		table: {
			...F,
			useDialog: !1,
			columnNames: {
				series: "Word",
				value: "Value"
			},
			th: N,
			td: {
				...P,
				roundingValue: 0,
				prefix: "",
				suffix: ""
			}
		}
	}, Oe = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		theme: "",
		customPalette: [],
		a11y: v,
		downsample: { threshold: 1e4 },
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			labels: !0,
			fullscreen: !0,
			stack: !0,
			annotator: !0
		}),
		style: {
			fontFamily: "Arial",
			chart: {
				backgroundColor: p,
				color: m,
				aspectRatio: "12 / 9",
				stacked: !1,
				stackGap: 20,
				scale: {
					ticks: 10,
					min: null,
					max: null
				},
				zoom: {
					...I,
					minimap: R,
					preview: {
						enable: !1,
						fill: "#CCCCCC50",
						stroke: "#6A6A6A",
						strokeWidth: 2,
						strokeDasharray: 0
					},
					useDefaultFormat: !0,
					timeFormat: "yyyy-MM-dd HH:mm:ss",
					customFormat: null
				},
				selector: {
					show: !0,
					color: t,
					dashed: !0,
					showHorizontalSelector: !1
				},
				tooltip: {
					...O,
					showTimeLabel: !0,
					useDefaultTimeFormat: !0,
					timeFormat: "yyyy-MM-dd HH:mm:ss"
				},
				legend: {
					...D,
					position: "bottom"
				},
				title: E,
				grid: {
					y: {
						showAxis: !0,
						axisColor: t,
						axisThickness: 2,
						axisName: "",
						axisLabels: {
							show: !0,
							fontSizeRatio: 1,
							color: m,
							offsetX: 0,
							rounding: 1,
							prefix: "",
							suffix: "",
							bold: !1
						},
						verticalLines: {
							show: !0,
							color: i,
							hideUnderXLength: 20,
							position: "middle"
						}
					},
					x: {
						showAxis: !0,
						axisColor: t,
						axisThickness: 2,
						axisName: "",
						horizontalLines: {
							show: !0,
							color: i,
							alternate: !0,
							opacity: 20
						},
						timeLabels: {
							show: !0,
							showMarker: !0,
							fontSizeRatio: .8,
							values: [],
							datetimeFormatter: A,
							rotation: 0,
							offsetY: 30,
							color: m,
							modulo: 12,
							bold: !1
						}
					},
					zeroLine: {
						show: !0,
						color: t,
						dashed: !0
					}
				},
				line: {
					cutNullValues: !0,
					plots: {
						show: !0,
						radiusRatio: 1
					}
				},
				bar: { gradient: { show: !0 } },
				area: { opacity: 60 },
				dataLabels: {
					show: !0,
					fontSizeRatio: 1,
					useSerieColor: !0,
					color: m,
					offsetY: -12,
					formatter: null,
					bold: !0
				},
				paddingProportions: {
					top: .1,
					right: .05,
					bottom: .1,
					left: .1
				}
			}
		},
		table: {
			...F,
			useDialog: !1,
			rounding: 1,
			columnNames: {
				period: "Period",
				total: "Total"
			},
			th: N,
			td: P
		}
	}, ke = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		customPalette: [],
		userOptions: j({
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			fullscreen: !0,
			annotator: !0,
			tooltip: !0,
			svg: !0
		}),
		nodeCategories: {},
		nodeCategoryColors: {},
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				width: 1e3,
				height: 600,
				color: m,
				padding: M([
					12,
					12,
					12,
					12
				]),
				title: E,
				tooltip: {
					...O,
					showPercentage: !0,
					roundingPercentage: 0,
					translations: {
						from: "From:",
						to: "To:",
						percentOfTotal: "Percent of total:"
					}
				},
				legend: {
					...D,
					position: "bottom"
				},
				nodes: {
					gap: 10,
					width: 40,
					labels: {
						show: !0,
						showValue: !0,
						fontSize: y._14,
						abbreviation: {
							use: !0,
							length: 3
						},
						prefix: "",
						suffix: "",
						rounding: 0,
						formatter: null
					},
					stroke: o,
					strokeWidth: 1,
					borderRadius: 0
				},
				links: {
					opacity: .8,
					stroke: o,
					strokeWidth: 1,
					smooth: !0
				}
			}
		},
		table: {
			...F,
			useDialog: !1,
			columnNames: {
				source: "Source",
				target: "Target",
				value: "Value"
			},
			th: N,
			td: P
		}
	}, Ae = {
		devHints: w,
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		transitions: k,
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		responsiveProportionalSizing: !0,
		theme: "",
		useCssAnimation: !1,
		customPalette: [],
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			labels: !0,
			fullscreen: !0,
			annotator: !0,
			svg: !0
		}),
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				height: 600,
				width: 1e3,
				padding: M([
					0,
					0,
					0,
					0
				]),
				comments: {
					show: !0,
					showInTooltip: !0,
					width: 200,
					offsetX: 0,
					offsetY: 0
				},
				lines: {
					smooth: !0,
					strokeWidth: 2,
					opacity: .8
				},
				plots: {
					show: !0,
					radius: 6,
					opacity: .8
				},
				yAxis: {
					scaleTicks: 10,
					stroke: t,
					strokeWidth: 1,
					labels: {
						showAxisNames: !0,
						axisNames: [],
						axisNamesRotation: 0,
						axisNamesAutoRotate: {
							enable: !0,
							angle: -30
						},
						axisNamesColor: m,
						axisNamesFontSize: y._16,
						axisNamesBold: !0,
						roundings: [],
						prefixes: [],
						suffixes: [],
						formatters: [],
						ticks: {
							show: !0,
							fontSize: y._14,
							color: m,
							bold: !1,
							offsetX: 0,
							offsetY: 0
						},
						datapoints: {
							show: !0,
							fontSize: y._14,
							useSerieColor: !0,
							color: m,
							offsetX: 0,
							offsetY: 0,
							bold: !0
						}
					}
				},
				title: E,
				legend: {
					...D,
					position: "bottom"
				},
				tooltip: O
			}
		},
		table: {
			...F,
			useDialog: !1,
			columnNames: {
				series: "Series",
				item: "Item"
			},
			th: N,
			td: P
		}
	}, je = {
		devHints: w,
		type: "stopwatch",
		responsive: !1,
		responsiveProportionalSizing: !0,
		useCursorPointer: !1,
		style: {
			fontFamily: "inherit",
			backgroundColor: p,
			height: 300,
			width: 300,
			title: E
		},
		stopwatch: {
			showHours: !1,
			showHundredth: !0,
			cycleSeconds: 5,
			track: {
				radiusRatio: 1,
				stroke: g,
				fill: o,
				strokeWidth: 2
			},
			tracker: {
				radiusRatio: 1,
				stroke: o,
				strokeWidth: 1,
				fill: t,
				gradient: {
					show: !0,
					color: o
				},
				aura: {
					show: !0,
					radiusRatio: 1,
					fill: t,
					stroke: o,
					strokeWidth: 0
				}
			},
			cycleTrack: {
				show: !0,
				stroke: t,
				strokeWidth: 3
			},
			label: {
				fontSize: y._24,
				color: m,
				bold: !1
			},
			legend: {
				backgroundColor: p,
				buttons: {
					start: !0,
					pause: !0,
					reset: !0,
					restart: !0,
					lap: !0,
					iconColor: t
				},
				buttonTitles: {
					start: "Start",
					pause: "Pause",
					resume: "Resume",
					reset: "Reset",
					restart: "Restart",
					lap: "Lap"
				}
			}
		}
	}, Me = {
		bubbleEffect: !0,
		bubbleEffectColor: o,
		bubbleEffectOpacity: .1,
		centerCircleColor: o,
		centerCircleDasharray: 0,
		centerCircleOpacity: 0,
		centerCircleRadius: 50,
		centerCircleStroke: i,
		centerCircleStrokeWidth: .5,
		coordinatesColor: i,
		coordinatesFontSize: y._10,
		coordinatesOffset: 0,
		crosshairDasharray: 0,
		crosshairStroke: i,
		crosshairStrokeWidth: .5,
		intersectCirclesFill: i,
		intersectCirclesRadius: 2,
		isLoading: !1,
		parentId: "",
		showCenterCircle: !0,
		showCoordinates: !0,
		showCrosshair: !0,
		showIntersectCircles: !0,
		useWaveOnClick: !0
	}, Ne = {
		open: !1,
		maxHeight: 2e3,
		useCursorPointer: !1,
		head: {
			useArrowSlot: !1,
			backgroundColor: p,
			color: m,
			iconColor: c,
			iconSize: 20,
			icon: "arrowRight",
			padding: "12px 6px"
		},
		body: {
			backgroundColor: p,
			color: m
		}
	}, Pe = {
		devHints: w,
		debug: !1,
		animationFrames: 60,
		animationValueStart: 0,
		backgroundColor: p,
		fontFamily: "inherit",
		layoutClass: "",
		layoutCss: "",
		prefix: "",
		suffix: "",
		title: "",
		titleBold: !0,
		titleColor: m,
		titleClass: "",
		titleCss: "",
		titleFontSize: y._16,
		useAnimation: !0,
		valueBold: !0,
		valueColor: c,
		valueClass: "",
		valueCss: "",
		valueFontSize: y._32,
		valueRounding: 0,
		formatter: null,
		analogDigits: {
			show: !1,
			height: 40,
			color: m,
			skeletonColor: g
		}
	}, $ = {
		devHints: w,
		type: "onion",
		onion: {
			gutterColor: i,
			gutterOpacity: .3,
			gutterBlur: 0,
			trackHueRotate: 20,
			trackBlur: 1,
			trackColor: d
		},
		line: {
			gutterColor: i,
			gutterOpacity: .3,
			gutterBlur: 0,
			trackHueRotate: 20,
			trackBlur: 1,
			trackColor: d
		},
		bar: {
			gutterColor: i,
			gutterOpacity: .3,
			gutterBlur: 0,
			trackHueRotate: 20,
			trackBlur: 1,
			trackColor: d
		}
	}, Fe = {
		debug: !1,
		devHints: w,
		readonly: !1,
		useCursorPointer: !1,
		style: {
			fontFamily: "inherit",
			itemSize: 32,
			backgroundColor: p,
			colors: {
				activeReadonly: [
					"#e20001",
					"#ff9f03",
					"#ffd004",
					"#61c900",
					"#059f00"
				],
				active: [
					"#e20001",
					"#ff9f03",
					"#ffd004",
					"#61c900",
					"#059f00"
				],
				inactive: [
					g,
					g,
					g,
					g,
					g
				]
			},
			icons: {
				filled: !1,
				useGradient: !0
			},
			title: {
				...E,
				textAlign: S.CENTER,
				offsetY: 6,
				subtitle: {
					...E.subtitle,
					offsetY: 12
				}
			},
			rating: {
				show: !0,
				fontSize: y._24,
				bold: !0,
				roundingValue: 1,
				position: S.BOTTOM,
				offsetX: 0,
				offsetY: 0,
				formatter: null
			},
			tooltip: {
				show: !0,
				fontSize: y._14,
				offsetY: 0,
				color: m,
				bold: !0,
				backgroundColor: p,
				borderColor: _,
				borderRadius: 4,
				boxShadow: "0 6px 12px -6px rgba(0,0,0,0.2)",
				roundingValue: 0,
				formatter: null
			}
		}
	}, Ie = {
		debug: !1,
		devHints: w,
		type: x.STAR,
		readonly: !1,
		from: 1,
		to: 5,
		useCursorPointer: !1,
		style: {
			fontFamily: "inherit",
			animated: !0,
			itemSize: 32,
			backgroundColor: p,
			star: {
				activeColor: f,
				borderColor: f,
				borderWidth: 3,
				apexes: 5,
				inactiveColor: g,
				useGradient: !0
			},
			image: {
				src: "",
				inactiveOpacity: .3,
				alt: "rating image"
			},
			title: {
				textAlign: S.CENTER,
				fontSize: y._20,
				color: m,
				bold: !0,
				text: "",
				offsetY: 6,
				subtitle: {
					fontSize: y._14,
					color: i,
					bold: !1,
					text: "",
					offsetY: 12
				}
			},
			rating: {
				show: !0,
				fontSize: y._24,
				bold: !0,
				roundingValue: 1,
				position: S.BOTTOM,
				offsetY: 0,
				offsetX: 0,
				formatter: null
			},
			tooltip: {
				show: !0,
				fontSize: y._14,
				offsetY: 0,
				color: m,
				bold: !0,
				backgroundColor: p,
				borderColor: _,
				borderRadius: 4,
				boxShadow: "0 6px 12px -6px rgba(0,0,0,0.2)",
				roundingValue: 0,
				formatter: null
			}
		}
	}, Le = {
		devHints: w,
		alwaysVisible: !1,
		useCursorPointer: !1,
		style: {
			backgroundColor: p,
			color: m,
			fontFamily: "inherit",
			showPrint: !1,
			showSave: !0,
			showImage: !0,
			showTooltips: !0,
			buttons: {
				borderRadius: 6,
				controls: {
					backgroundColor: p,
					color: m,
					border: "1px solid #262626",
					selected: {
						backgroundColor: t,
						color: s,
						border: `1px solid ${_}`
					}
				},
				shapes: {
					backgroundColor: p,
					color: m,
					border: "1px solid #262626",
					selected: {
						backgroundColor: m,
						color: p,
						border: `1px solid ${_}`
					}
				}
			},
			tooltips: {
				backgroundColor: s,
				color: m,
				border: `1px solid ${_}`,
				borderRadius: 6,
				boxShadow: "0 6px 12px -6px rgba(0,0,0,0.2)"
			}
		},
		translations: {
			colorAlpha: "Color alpha",
			dashedLines: "Dashed lines",
			filled: "Filled",
			fontSize: "Font size",
			thickness: "Thickness",
			title: "Annotations",
			tooltipGroup: "Select & group",
			tooltipDelete: "Delete",
			tooltipMove: "Move",
			tooltipResize: "Resize",
			tooltipBringToFront: "Bring to front",
			tooltipBringToBack: "Bring to back",
			tooltipDuplicate: "Duplicate",
			tooltipUndo: "Undo last shape",
			tooltipRedo: "Redo last shape",
			tooltipPdf: "Save pdf",
			tooltipSave: "Save annotations",
			tooltipShapeCircle: "Draw circle",
			tooltipShapeRect: "Draw rect",
			tooltipShapeArrow: "Draw arrow",
			tooltipShapeFreehand: "Freehand line",
			tooltipShapeText: "Text mode",
			tooltipShapeTextLeft: "Align left",
			tooltipShapeTextCenter: "Align center",
			tooltipShapeTextRight: "Align right",
			tooltipShapeTextBullet: "Bullet points",
			tooltipShapeTextBold: "Bold",
			tooltipShapeTextItalic: "Italic",
			tooltipShapeTextUnderline: "Underlined",
			tooltipShapeColor: "Color",
			tooltipImage: "Download PNG"
		}
	}, Re = {
		devHints: w,
		locked: !1,
		style: {
			board: {
				backgroundColor: p,
				color: m,
				aspectRatio: "1/1.4141",
				border: "none"
			},
			item: {
				backgroundColor: p,
				borderColor: _
			},
			resizeHandles: { backgroundColor: m }
		},
		userOptions: {
			show: !0,
			useCursorPointer: !1,
			showOnChartHover: !1,
			keepStateOnChartLeave: !0,
			position: "right",
			buttons: {
				pdf: !0,
				img: !0,
				annotator: !0,
				altCopy: !1
			},
			callbacks: {
				pdf: null,
				img: null,
				annotator: null,
				altCopy: null
			},
			buttonTitles: {
				pdf: "Download PDF",
				img: "Download PNG",
				annotator: "Toggle annotator",
				altCopy: "Copy alt text"
			},
			print: {
				scale: 2,
				filename: ""
			}
		}
	}, ze = {
		devHints: w,
		type: x.LINE,
		style: {
			backgroundColor: p,
			color: m,
			maxHeight: 500,
			animated: !0,
			chord: { color: g },
			ridgeline: { color: g },
			circlePack: { color: g },
			historyPlot: { color: g },
			bullet: { color: g },
			flow: { color: g },
			parallelCoordinatePlot: { color: g },
			treemap: { color: g },
			dumbbell: { color: g },
			stripPlot: { color: g },
			galaxy: { color: g },
			bar3d: { color: g },
			sparkHistogram: { color: g },
			sparkStackbar: { color: g },
			sparkbar: { color: g },
			thermometer: { color: g },
			relationCircle: { color: g },
			molecule: { color: g },
			tiremarks: { color: g },
			chestnut: { color: g },
			sparkline: {
				color: g,
				strokeWidth: .7
			},
			donutEvolution: {
				axis: {
					show: !0,
					color: g,
					strokeWidth: .5
				},
				donuts: {
					strokeWidth: .5,
					color: g
				}
			},
			line: {
				axis: {
					show: !0,
					color: g,
					strokeWidth: .5
				},
				path: {
					color: g,
					strokeWidth: 1,
					showPlots: !0
				}
			},
			bar: {
				axis: {
					show: !0,
					color: g,
					strokeWidth: .5
				},
				borderRadius: .5,
				color: g,
				barWidth: 9
			},
			donut: {
				color: g,
				strokeWidth: 64
			},
			onion: { color: g },
			gauge: { color: g },
			quadrant: {
				grid: {
					color: g,
					strokeWidth: .5
				},
				plots: {
					radius: 1.5,
					color: g
				}
			},
			radar: {
				grid: {
					color: g,
					strokeWidth: .5
				},
				shapes: { color: g }
			},
			waffle: { color: g },
			table: {
				th: { color: g },
				td: {
					color: g,
					strokeWidth: .5
				}
			},
			rating: {
				useSmiley: !1,
				color: g,
				filled: !0,
				strokeWidth: 1,
				maxWidth: 200
			},
			verticalBar: {
				axis: {
					show: !0,
					color: g,
					strokeWidth: .5
				},
				borderRadius: .5,
				color: g
			},
			heatmap: {
				cellsX: 26,
				cellsY: 7,
				color: g
			},
			candlesticks: {
				axis: {
					show: !0,
					color: g,
					strokeWidth: .5
				},
				candle: {
					color: g,
					strokeWidth: 1
				}
			},
			pyramid: { color: g },
			wheel: { color: g },
			rings: { color: g }
		}
	}, Be = {
		devHints: w,
		fontFamily: "inherit",
		maxHeight: 500,
		rowsPerPage: 25,
		useCursorPointer: !1,
		style: {
			title: {
				...E,
				backgroundColor: p
			},
			th: {
				backgroundColor: g,
				color: m,
				outline: `1px solid ${o}`,
				selected: {
					backgroundColor: c,
					color: o
				},
				buttons: {
					filter: {
						inactive: {
							backgroundColor: g,
							color: m
						},
						active: {
							backgroundColor: c,
							color: o
						}
					},
					cancel: {
						inactive: {
							backgroundColor: g,
							color: t
						},
						active: {
							backgroundColor: "#F17171",
							color: o
						}
					}
				}
			},
			rows: {
				outline: `1px solid ${p}`,
				even: {
					backgroundColor: "#f3f5f7",
					color: m,
					selectedCell: {
						backgroundColor: "#1f77b45b",
						color: m
					},
					selectedNeighbors: {
						backgroundColor: "#63dd821e",
						color: m
					}
				},
				odd: {
					backgroundColor: p,
					color: m,
					selectedCell: {
						backgroundColor: "#1f77b45b",
						color: m
					},
					selectedNeighbors: {
						backgroundColor: "#63dd821e",
						color: m
					}
				}
			},
			inputs: {
				backgroundColor: p,
				color: m,
				border: `1px solid ${_}`,
				accentColor: c
			},
			dropdowns: {
				backgroundColor: g,
				color: m,
				icons: {
					selected: {
						color: u,
						unicode: "✔"
					},
					unselected: {
						color: l,
						unicode: "✖"
					}
				}
			},
			infoBar: {
				backgroundColor: g,
				color: m
			},
			pagination: {
				buttons: {
					backgroundColor: g,
					color: m,
					opacityDisabled: .5
				},
				navigationIndicator: { backgroundColor: c }
			},
			exportMenu: {
				show: !0,
				backgroundColor: g,
				color: m,
				buttons: {
					backgroundColor: s,
					color: m
				},
				filename: ""
			},
			closeButtons: {
				backgroundColor: "transparent",
				color: m,
				borderRadius: "50%"
			},
			chart: {
				modal: {
					backgroundColor: g,
					color: m,
					buttons: {
						selected: {
							backgroundColor: c,
							color: o
						},
						unselected: {
							backgroundColor: p,
							color: m
						}
					}
				},
				layout: {
					backgroundColor: p,
					axis: {
						stroke: "#ccd1d4",
						strokeWidth: 2
					},
					bar: {
						fill: c,
						stroke: o
					},
					line: {
						smooth: !0,
						useArea: !1,
						stroke: c,
						strokeWidth: 4,
						plot: {
							fill: c,
							stroke: o,
							strokeWidth: 1,
							radius: {
								selected: 6,
								unselected: 4
							}
						},
						selector: {
							stroke: "#ccc",
							strokeWidth: 1,
							strokeDasharray: 5
						}
					},
					labels: { color: m },
					progression: {
						stroke: t,
						strokeWidth: 2,
						strokeDasharray: 4,
						arrowSize: 7
					},
					timeLabels: {
						showOnlyAtModulo: !0,
						modulo: 12
					},
					datetimeFormatter: {
						...A,
						enable: !0
					},
					zoom: {
						show: !0,
						autoFit: !0
					}
				}
			}
		},
		translations: {
			average: "Average",
			by: "by",
			chooseCategoryColumn: "Choose category column",
			exportAllButton: "CSV all",
			exportAllLabel: "Export all rows of your current filtered dataset",
			exportPageButton: "CSV page",
			exportPageLabel: "Export rows of the current page",
			from: "From",
			inputPlaceholder: "Search...",
			makeDonut: "Generate",
			nb: "Nb",
			page: "Page",
			paginatorLabel: "Rows per page",
			sizeWarning: "Displaying too many rows at a time can lead to slower performance",
			sum: "Sum",
			to: "To",
			total: "Total",
			totalRows: "Total rows",
			filename: "File name",
			xAxisLabels: "X axis labels"
		},
		useChart: !0
	}, Ve = {
		devHints: w,
		height: "100%",
		width: null,
		backgroundColor: p,
		digits: {
			color: m,
			skeletonColor: g,
			thickness: 1
		}
	}, He = {
		debug: !1,
		devHints: w,
		responsiveBreakpoint: 400,
		userOptions: j({
			tooltip: !1,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !1,
			labels: !1,
			fullscreen: !0,
			stack: !1,
			animation: !0
		}),
		animation: {
			type: "scroll",
			use: !0,
			speedMs: 1e3,
			pauseOnHover: !0
		},
		style: {
			backgroundColor: p,
			color: m,
			fontFamily: "inherit"
		},
		border: {
			size: 0,
			color: t
		},
		caption: {
			text: "",
			padding: M([
				12,
				12,
				12,
				12
			]),
			style: {
				backgroundColor: p,
				color: m,
				fontSize: `${y._16}px`,
				fontWeight: "bold",
				textAlign: "left"
			}
		},
		scrollbar: {
			showOnlyOnHover: !1,
			hide: !1
		},
		thead: {
			style: { verticalAlign: "middle" },
			tr: {
				height: 32,
				border: {
					size: 0,
					color: t
				},
				style: {
					backgroundColor: p,
					color: m,
					boxShadow: `0px 6px 12px -6px ${t}50`
				},
				th: {
					border: {
						size: 0,
						color: t
					},
					padding: M([
						0,
						12,
						0,
						0
					]),
					style: {
						borderSpacing: 0,
						border: "none",
						textAlign: "right",
						fontVariantNumeric: "tabular-nums"
					}
				}
			}
		},
		tbody: {
			backgroundColor: p,
			tr: {
				visible: 5,
				height: 32,
				border: {
					size: 0,
					color: t
				},
				style: {
					backgroundColor: p,
					color: m
				},
				td: {
					alternateColor: !0,
					alternateOpacity: .5,
					border: {
						size: 0,
						color: t
					},
					padding: M([
						0,
						12,
						0,
						0
					]),
					style: {
						fontVariantNumeric: "tabular-nums",
						textAlign: "right",
						backgroundColor: g
					}
				}
			}
		}
	}, Ue = {
		devHints: w,
		a11y: { translations: { label: "Progress" } },
		skeletonConfig: null,
		skeletonDataset: null,
		debug: !1,
		loading: !1,
		type: "battery",
		size: 64,
		stroke: i,
		color: c,
		useGradient: !0,
		gradientColor: "#9db5ed",
		showPercentage: !0,
		textColor: t,
		fontFamily: "inherit",
		formatter: null
	}, We = {
		devHints: w,
		debug: !1,
		loading: !1,
		responsive: !1,
		theme: "",
		userOptions: j({
			tooltip: !1,
			pdf: !0,
			csv: !1,
			img: !0,
			table: !1,
			labels: !1,
			fullscreen: !0,
			stack: !1,
			annotator: !0,
			svg: !0
		}),
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				height: 96,
				width: 600,
				padding: M([
					24,
					24,
					24,
					12
				]),
				animation: {
					show: !0,
					animationFrames: 60
				},
				segments: {
					baseColor: "#9A9A9A",
					dataLabels: {
						show: !0,
						color: m,
						fontSize: y._10,
						formatter: null,
						bold: !1,
						prefix: "",
						suffix: "",
						rounding: 0,
						offsetY: 0
					},
					ticks: {
						show: !0,
						divisions: 10,
						stroke: "#8A8A8A"
					}
				},
				target: {
					show: !0,
					onTop: !0,
					color: m,
					rounded: !0,
					heightRatio: .8,
					stroke: o,
					strokeWidth: 1,
					width: 6
				},
				valueBar: {
					color: "#3A3A3A",
					heightRatio: .33,
					stroke: o,
					strokeWidth: 1,
					label: {
						show: !0,
						color: m,
						fontSize: y._14,
						bold: !0,
						offsetY: 0
					}
				},
				title: {
					...E,
					textAlign: S.LEFT,
					paddingLeft: 12
				},
				legend: {
					...D,
					roundingValue: 0,
					position: "bottom"
				}
			}
		}
	}, Ge = {
		debug: !1,
		devHints: w,
		theme: "",
		responsive: !1,
		responsiveProportionalSizing: !0,
		useCssAnimation: !0,
		table: {
			...F,
			useDialog: !1,
			th: N,
			td: {
				...P,
				roundingValue: 0,
				roundingPercentage: 0
			},
			columnNames: {
				series: "Step",
				value: "Value",
				percentage: "Percentage"
			}
		},
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			labels: !1,
			fullscreen: !0,
			stack: !1,
			annotator: !0,
			svg: !0
		}),
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				width: 600,
				height: 500,
				title: E,
				padding: M([
					12,
					128,
					24,
					24
				]),
				barCircleSpacingRatio: .05,
				circles: {
					stroke: o,
					strokeWidth: 1,
					dataLabels: {
						formatter: null,
						fontSize: y._16,
						offsetY: 0,
						adaptColorToBackground: !0,
						color: m,
						rounding: 1,
						bold: !0
					}
				},
				circleLinks: {
					show: !0,
					color: g,
					widthRatio: 1
				},
				area: {
					show: !0,
					color: g
				},
				bars: {
					stroke: o,
					defaultColor: c,
					strokeWidth: 1,
					gapRatio: .2,
					borderRadius: 3,
					dataLabels: {
						name: {
							fontSize: y._16,
							color: m,
							bold: !0,
							offsetX: 0,
							offsetY: 0
						},
						value: {
							formatter: null,
							fontSize: y._16,
							rounding: 0,
							bold: !1,
							color: m,
							prefix: "",
							suffix: "",
							offsetX: 0,
							offsetY: 0
						}
					}
				}
			}
		}
	}, Ke = {
		devHints: w,
		skeletonDataset: null,
		skeletonConfig: null,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		transitions: k,
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		responsiveProportionalSizing: !0,
		theme: "",
		customPalette: [],
		useCssAnimation: !1,
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			labels: !1,
			fullscreen: !0,
			stack: !1,
			annotator: !0,
			svg: !0
		}),
		table: {
			...F,
			useDialog: !1,
			th: N,
			td: {
				...P,
				roundingValue: 0
			},
			columnNames: {
				series: "Series",
				datapoint: "Datapoint",
				x: "x",
				y: "y"
			}
		},
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				height: 500,
				width: 600,
				padding: M([
					12,
					12,
					12,
					12
				]),
				grid: {
					xAxis: {
						show: !0,
						stroke: g,
						strokeWidth: 1
					},
					horizontalLines: {
						show: !0,
						stroke: g,
						strokeWidth: .6
					},
					yAxis: {
						show: !0,
						stroke: g,
						strokeWidth: 1
					},
					verticalLines: {
						show: !0,
						stroke: g,
						strokeWidth: .6
					}
				},
				axes: {
					x: {
						scaleMin: null,
						scaleMax: null,
						ticks: 10,
						labels: {
							show: !0,
							fontSize: y._16,
							color: m,
							bold: !1,
							rounding: 1,
							offsetY: 0,
							rotation: 0,
							autoRotate: {
								enable: !0,
								angle: -30
							},
							formatter: null,
							prefix: "",
							suffix: ""
						},
						name: {
							text: "",
							fontSize: y._16,
							offsetX: 0,
							offsetY: 0,
							bold: !1,
							color: m
						}
					},
					y: {
						scaleMin: null,
						scaleMax: null,
						ticks: 10,
						labels: {
							show: !0,
							fontSize: y._16,
							color: m,
							bold: !1,
							rounding: 1,
							offsetX: 0,
							formatter: null,
							prefix: "",
							suffix: ""
						},
						name: {
							text: "",
							fontSize: y._16,
							offsetX: 0,
							offsetY: 0,
							bold: !1,
							color: m
						}
					}
				},
				plots: {
					radius: 16,
					stroke: o,
					strokeWidth: 1,
					unselectedOpacity: .1,
					gradient: {
						show: !0,
						intensity: 40
					},
					indexLabels: {
						show: !0,
						startAtZero: !1,
						adaptColorToBackground: !0,
						color: m,
						fontSize: y._16,
						bold: !1,
						offsetY: 0,
						offsetX: 0
					},
					labels: {
						show: !0,
						fontSize: y._10,
						color: m,
						bold: !1,
						offsetY: 0,
						offsetX: 0
					}
				},
				paths: {
					show: !0,
					strokeWidth: 1.6,
					useSerieColor: !0,
					stroke: t
				},
				legend: {
					...D,
					position: "bottom"
				},
				title: E,
				tooltip: O
			}
		}
	}, qe = {
		devHints: w,
		debug: !1,
		loading: !1,
		responsive: !1,
		a11y: {
			...v,
			translations: {
				...v.translations,
				keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
			}
		},
		transitions: k,
		events: {
			datapointEnter: null,
			datapointLeave: null,
			datapointClick: null
		},
		theme: "",
		customPalette: [],
		userOptions: j({
			tooltip: !0,
			pdf: !0,
			csv: !0,
			img: !0,
			table: !0,
			labels: !1,
			fullscreen: !0,
			stack: !1,
			annotator: !0,
			svg: !0,
			zoom: !0
		}),
		table: {
			...F,
			useDialog: !1,
			th: N,
			td: P,
			columnNames: {
				datapoint: "Datapoint",
				parent: "Parent",
				depth: "Depth",
				value: "Value"
			}
		},
		style: {
			fontFamily: "inherit",
			chart: {
				backgroundColor: p,
				color: m,
				title: E,
				width: 512,
				height: 316,
				circles: {
					stroke: o,
					strokeWidth: 1,
					selectedShadowColor: t,
					gradient: {
						show: !0,
						intensity: 40
					},
					labels: {
						parents: { show: !1 },
						children: { show: !0 },
						name: {
							fontSizeRatio: 1,
							show: !0,
							bold: !1,
							offsetY: 0,
							color: "auto"
						},
						value: {
							fontSizeRatio: 1,
							show: !0,
							color: "auto",
							rounding: 0,
							prefix: "",
							suffix: "",
							formatter: null,
							bold: !1,
							offsetY: 0
						}
					}
				},
				parentTooltips: {
					show: !0,
					fontSizeRatio: 1,
					borderRadiusRatio: 1,
					color: t,
					filter: "drop-shadow(0 2px 4px #2D353C30)",
					backgroundColor: o,
					strokeWidth: 1,
					useSerieColor: !0,
					stroke: t,
					link: {
						strokeWidth: 1,
						strokeDasharray: 0,
						opacity: .6,
						useSerieColor: !1,
						stroke: t
					}
				},
				tooltip: O,
				zoom: {
					show: !0,
					speed: 1
				}
			}
		}
	};
	return {
		vue_ui_xy: V,
		vue_ui_donut: H,
		vue_ui_treemap: U,
		vue_ui_waffle: W,
		vue_ui_radar: G,
		vue_ui_quadrant: K,
		vue_ui_gauge: q,
		vue_ui_wheel: J,
		vue_ui_tiremarks: Y,
		vue_ui_chestnut: X,
		vue_ui_onion: Z,
		vue_ui_vertical_bar: Q,
		vue_ui_horizontal_bar: ne,
		vue_ui_heatmap: re,
		vue_ui_scatter: ie,
		vue_ui_candlestick: ae,
		vue_ui_sparkline: oe,
		vue_ui_sparkbar: se,
		vue_ui_sparkstackbar: ce,
		vue_ui_sparkhistogram: le,
		vue_ui_sparkgauge: ue,
		vue_ui_spark_trend: de,
		vue_ui_quick_chart: fe,
		vue_ui_age_pyramid: pe,
		vue_ui_relation_circle: me,
		vue_ui_thermometer: he,
		vue_ui_rings: ge,
		vue_ui_donut_evolution: _e,
		vue_ui_mood_radar: ve,
		vue_ui_molecule: ye,
		vue_ui_nested_donuts: be,
		vue_ui_galaxy: xe,
		vue_ui_strip_plot: Se,
		vue_ui_dumbbell: Ce,
		vue_ui_3d_bar: we,
		vue_ui_table_sparkline: Te,
		vue_ui_table_heatmap: Ee,
		vue_ui_word_cloud: De,
		vue_ui_xy_canvas: Oe,
		vue_ui_flow: ke,
		vue_ui_parallel_coordinate_plot: Ae,
		vue_ui_timer: je,
		vue_ui_carousel_table: He,
		vue_ui_gizmo: Ue,
		vue_ui_stackbar: te,
		vue_ui_bullet: We,
		vue_ui_funnel: Ge,
		vue_ui_history_plot: Ke,
		vue_ui_world: {
			devHints: w,
			skeletonDataset: null,
			skeletonConfig: null,
			debug: !1,
			loading: !1,
			events: {
				datapointEnter: null,
				datapointLeave: null,
				datapointClick: null
			},
			userOptions: j({
				tooltip: !0,
				pdf: !0,
				csv: !0,
				img: !0,
				table: !0,
				labels: !1,
				fullscreen: !0,
				stack: !1,
				annotator: !0,
				svg: !0
			}),
			customPalette: [],
			projection: "globe",
			style: {
				fontFamily: "inherit",
				chart: {
					backgroundColor: p,
					color: m,
					padding: M([
						12,
						12,
						12,
						12
					]),
					dataLabels: {
						prefix: "",
						suffix: "",
						rounding: 0,
						formatter: null
					},
					dimensions: {
						height: null,
						width: null
					},
					globe: {
						center: {
							x: 0,
							y: 0
						},
						waterColor: c
					},
					territory: {
						stroke: "#666666",
						emptyColor: "#E0E0E0",
						strokeWidth: .5,
						strokeWidthSelected: 2,
						colors: {
							min: "#E0E0E0",
							max: null
						},
						showTaiwanAsPartOfChina: !1
					},
					tooltip: {
						...O,
						showMinimap: !0
					},
					title: E,
					legend: {
						...D,
						position: "bottom"
					}
				}
			},
			table: {
				...F,
				useDialog: !1,
				th: N,
				td: {
					...P,
					roundingValue: 0
				},
				columnNames: {
					series: "Country",
					value: "Value",
					category: "Category"
				}
			}
		},
		vue_ui_ridgeline: {
			devHints: w,
			skeletonDataset: null,
			skeletonConfig: null,
			debug: !1,
			loading: !1,
			responsive: !1,
			a11y: {
				...v,
				translations: {
					...v.translations,
					keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
				}
			},
			events: {
				datapointEnter: null,
				datapointLeave: null,
				datapointClick: null
			},
			theme: "",
			customPalette: [],
			useCssAnimation: !1,
			userOptions: j({
				tooltip: !1,
				pdf: !0,
				csv: !0,
				img: !0,
				table: !0,
				labels: !1,
				fullscreen: !0,
				stack: !1,
				annotator: !0,
				svg: !0
			}),
			table: {
				...F,
				useDialog: !1,
				th: N,
				td: {
					...P,
					roundingValue: 0
				},
				columnNames: { series: "Series" }
			},
			style: {
				fontFamily: "inherit",
				chart: {
					backgroundColor: p,
					color: m,
					padding: M([
						12,
						0,
						0,
						12
					]),
					title: E,
					legend: D,
					dialog: {
						show: !0,
						backgroundColor: p,
						color: m,
						header: {
							backgroundColor: g,
							color: m
						},
						xyChart: {
							...V,
							responsive: !0,
							line: {
								...V.line,
								showTransition: !1,
								labels: {
									...V.line.labels,
									show: !0
								}
							},
							chart: {
								...V.chart,
								tooltip: {
									...V.chart.tooltip,
									showPercentage: !1
								},
								userOptions: {
									...V.chart.userOptions,
									buttons: {
										...V.chart.userOptions.buttons,
										pdf: !1,
										fullscreen: !1
									}
								},
								zoom: {
									...V.chart.zoom,
									autoFit: !0,
									minimap: {
										...V.chart.zoom.minimap,
										show: !0
									}
								}
							}
						}
					},
					areas: {
						height: 60,
						rowHeight: 30,
						strokeWidth: 1,
						stroke: {
							useSerieColor: !1,
							color: o
						},
						smooth: !0,
						opacity: .9,
						useCommonColor: !1,
						useGradient: !0,
						maxPoint: {
							show: !0,
							adaptStrokeToBackground: !0,
							stroke: t,
							strokeWidth: 1,
							strokeDasharray: 4
						}
					},
					selector: {
						show: !0,
						stroke: i,
						strokeWidth: 1,
						strokeDasharray: 2,
						labels: {
							fontSize: 12,
							formatter: null,
							rounding: 0,
							color: m
						},
						dot: {
							radius: 4,
							useDatapointColor: !0,
							fill: i,
							stroke: o,
							strokeWidth: .5
						}
					},
					zeroLine: {
						show: !0,
						strokeWidth: 1,
						strokeDasharray: 0,
						useSerieColor: !1,
						stroke: i
					},
					xAxis: { labels: {
						prefix: "",
						suffix: "",
						rotation: 0,
						autoRotate: {
							enable: !0,
							angle: -30
						},
						values: [],
						datetimeFormatter: A,
						color: m,
						fontSize: 14,
						bold: !1,
						showOnlyAtModulo: !1,
						modulo: 12,
						showOnlyFirstAndLast: !1,
						offsetY: 0
					} },
					yAxis: { labels: {
						fontSize: 16,
						bold: !1,
						color: m,
						offsetX: 0
					} }
				}
			}
		},
		vue_ui_chord: {
			devHints: w,
			debug: !1,
			loading: !1,
			responsive: !1,
			a11y: {
				...v,
				translations: {
					...v.translations,
					keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
				}
			},
			events: {
				datapointEnter: null,
				datapointLeave: null,
				datapointClick: null
			},
			theme: "",
			customPalette: [],
			enableRotation: !0,
			initialRotation: 0,
			useCssAnimation: !1,
			userOptions: j({
				tooltip: !1,
				pdf: !0,
				csv: !0,
				img: !0,
				table: !0,
				labels: !1,
				fullscreen: !0,
				stack: !1,
				annotator: !0,
				svg: !0
			}),
			table: {
				...F,
				useDialog: !1,
				th: N,
				td: P
			},
			style: {
				fontFamily: "inherit",
				chart: {
					backgroundColor: p,
					color: m,
					legend: {
						...D,
						position: "bottom"
					},
					title: E,
					arcs: {
						innerRadiusRatio: 1,
						outerRadiusRatio: 1,
						padAngle: 5,
						stroke: o,
						strokeWidth: 1,
						labels: {
							show: !0,
							fontSize: 14,
							bold: !1,
							curved: !1,
							adaptColorToBackground: !0,
							color: m,
							offset: 0,
							showPercentage: !0,
							roundingPercentage: 0
						}
					},
					ribbons: {
						stroke: o,
						strokeWidth: 1,
						underlayerOpacity: 1,
						labels: {
							show: !0,
							formatter: null,
							prefix: "",
							suffix: "",
							rounding: 0,
							fontSize: 14,
							bold: !1,
							useSerieColor: !1,
							color: m,
							offset: 0,
							minSeparationDeg: 3,
							connector: {
								stroke: t,
								strokeWidth: 1
							},
							marker: {
								show: !0,
								radius: 3,
								stroke: o,
								strokeWidth: 1
							}
						}
					}
				}
			}
		},
		vue_ui_stackline: B,
		vue_ui_dag: {
			devHints: w,
			skeletonConfig: null,
			skeletonDataset: null,
			loading: !1,
			debug: !1,
			responsive: !1,
			theme: "",
			a11y: {
				...v,
				translations: {
					...v.translations,
					keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
				}
			},
			userOptions: j({
				tooltip: !1,
				pdf: !0,
				csv: !1,
				img: !0,
				fullscreen: !0,
				annotator: !0,
				svg: !0,
				zoom: !0
			}),
			style: {
				fontFamily: "inherit",
				chart: {
					backgroundColor: p,
					color: m,
					width: null,
					height: null,
					backgroundPattern: {
						show: !1,
						spacingRatio: 3,
						dotRadiusRatio: 5,
						dotColor: g,
						opacity: 1
					},
					layout: {
						rankDirection: "TB",
						rankSeparation: 60,
						nodeSeparation: 50,
						edgeSeparation: 30,
						nodeWidth: 100,
						nodeHeight: 40,
						curvedEdges: !0,
						padding: 48,
						arrowShape: "vee",
						arrowSize: 8,
						align: void 0
					},
					nodes: {
						stroke: i,
						strokeWidth: 1,
						borderRadius: 3,
						backgroundColor: p,
						labels: {
							color: m,
							fontSize: y._12,
							bold: !1
						},
						tooltip: {
							showOnClick: !1,
							backgroundColor: g,
							color: m,
							maxWidth: "300px"
						},
						selected: {
							stroke: null,
							strokeWidth: null,
							backgroundColor: null,
							labelColor: null,
							downstreamEdges: {
								stroke: null,
								animated: null
							},
							upstreamEdges: {
								stroke: null,
								animated: null
							}
						}
					},
					edges: {
						stroke: i,
						strokeWidth: 1,
						animations: {
							dasharray: "2 6",
							animationDurationMs: 1e3
						}
					},
					midpoints: {
						show: !1,
						radius: 4,
						stroke: i,
						fill: o,
						strokeWidth: 1,
						tooltip: {
							maxWidth: "300px",
							backgroundColor: g,
							color: m
						},
						selectedEdge: {
							stroke: null,
							animated: null
						}
					},
					controls: {
						position: "bottom",
						show: !0,
						backgroundColor: g,
						buttonColor: g,
						color: m,
						fontSize: 14,
						border: `1px solid ${_}`,
						padding: "0.5rem",
						borderRadius: "0.25rem"
					},
					zoom: { active: !0 },
					title: E
				}
			}
		},
		vue_ui_geo: {
			devHints: w,
			skeletonDataset: null,
			skeletonConfig: null,
			loading: !1,
			debug: !1,
			responsive: !1,
			projection: "equirectangular",
			theme: "",
			a11y: {
				...v,
				translations: {
					...v.translations,
					keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
				}
			},
			userOptions: j({
				tooltip: !0,
				pdf: !0,
				csv: !1,
				img: !0,
				svg: !0,
				table: !1,
				labels: !1,
				fullscreen: !0,
				annotator: !0,
				zoom: !0
			}),
			map: {
				geoJson: null,
				center: [0, 0],
				fitPadding: 0
			},
			events: {
				datapointEnter: null,
				datapointLeave: null,
				datapointClick: null,
				territoryEnter: null,
				territoryLeave: null,
				territoryClick: null
			},
			style: {
				fontFamily: "inherit",
				chart: {
					dimensions: {
						width: null,
						height: null
					},
					backgroundColor: p,
					color: m,
					territory: {
						fill: "#F2F3F5",
						stroke: "#D0D4D8",
						strokeWidth: 1,
						hover: {
							enabledWhenEmpty: !1,
							fill: g,
							stroke: i,
							strokeWidth: 1.5
						}
					},
					points: {
						radius: 1,
						stroke: o,
						strokeWidth: 1,
						fill: a,
						hoverRadiusRatio: 1.2,
						labels: {
							show: !0,
							fontSizeRatio: 1,
							color: m,
							offsetY: 0
						}
					},
					controls: {
						position: "bottom",
						show: !0,
						backgroundColor: g,
						buttonColor: g,
						color: m,
						fontSize: 14,
						border: `1px solid ${_}`,
						padding: "0.5rem",
						borderRadius: "0.25rem"
					},
					title: E,
					tooltip: O,
					zoom: { active: !0 }
				}
			}
		},
		vue_ui_bump: {
			devHints: w,
			skeletonConfig: null,
			skeletonDataset: null,
			loading: !1,
			debug: !1,
			responsive: !1,
			theme: "",
			customPalette: [],
			useCssAnimation: !1,
			a11y: {
				...v,
				translations: {
					...v.translations,
					keyboardNavigation: "Use the left and right, or up and down arrow keys to move between datapoints"
				}
			},
			events: {
				datapointEnter: null,
				datapointLeave: null,
				datapointClick: null
			},
			userOptions: j({
				tooltip: !1,
				pdf: !0,
				csv: !0,
				img: !0,
				svg: !0,
				table: !0,
				labels: !1,
				fullscreen: !0,
				sort: !1,
				stack: !1,
				animation: !1,
				annotator: !0,
				zoom: !1
			}),
			table: {
				...F,
				useDialog: !1,
				columnNames: {
					series: "Series",
					period: "Period",
					values: "Values",
					ranking: "Ranking"
				},
				th: N,
				td: {
					...P,
					roundingValue: 0
				}
			},
			style: {
				fontFamily: "inherit",
				chart: {
					backgroundColor: p,
					color: m,
					height: 500,
					width: 800,
					layout: {
						timeLabels: {
							show: !0,
							values: [],
							datetimeFormatter: A,
							offsetY: 0,
							rotation: 0,
							autoRotate: {
								enable: !0,
								angle: -30
							},
							fontSize: y._14,
							color: m,
							bold: !1,
							showOnlyFirstAndLast: !1,
							showOnlyAtModulo: !1,
							modulo: 12
						},
						lines: {
							smooth: !0,
							strokeWidth: 4,
							coatingColor: o
						},
						plots: {
							stroke: o,
							strokeWidth: 1,
							radius: 12,
							labels: {
								color: "auto",
								show: !0,
								bold: !0,
								displayedValue: "value",
								fontSize: y._12,
								prefix: "",
								suffix: "",
								rounding: 0,
								formatter: null
							}
						},
						nameLabels: {
							fontSize: y._14,
							color: m,
							useSerieColor: !1,
							bold: !1,
							offsetX: 0,
							leftLabels: { show: !0 },
							rightLabels: { show: !0 }
						}
					},
					padding: M([
						12,
						12,
						12,
						12
					]),
					title: E
				}
			}
		},
		vue_ui_circle_pack: qe,
		vue_ui_cursor: Me,
		vue_ui_accordion: Ne,
		vue_ui_kpi: Pe,
		vue_ui_mini_loader: $,
		vue_ui_smiley: Fe,
		vue_ui_rating: Ie,
		vue_ui_annotator: Le,
		vue_ui_dashboard: Re,
		vue_ui_skeleton: ze,
		vue_ui_table: Be,
		vue_ui_digits: Ve,
		vue_ui_hill: z
	};
}
//#endregion
export { e as t };
