# TriclusteringToolbox
OAC-Triclustering instruments

This is a companion page for the paper on Triclustering of Real-Valued Triadic Contexts.


Таблица 1. Контекст GroupLens

|           |	#G     | #M   |	#B    |	Количество троек   |	Плотность контекста   |
| --------- | ----- | ----- | ----- | ------------------ | ---------------------- |
| GroupLens	| 1000  | 1700  |	19    |	  212595           |       	0,00658         |


Таблица 2. Результаты экспериментов на отсутствие данных

| % потерь |	Количества трикластеров Tri-k-means |	Обнаружение исходных кубоидов Tri-k-means |	Количество трикластеров NOAC	| Обнаружение исходных кубоидов NOAC |
| -- | -- | -- | -- | -------- |
| 0  |	2| 	Да |	2 |	Да |
| 10 |	2  |	Да |	603	| Да |
| 20 |	2	 | Да	| 862	| Нет |
| 30	| 2	| Да	| 771	| Нет |
| 40	| 2	| Да	| 664	| Нет |
| 50	| 2	| Да	| 554	| Нет |
| 60	| 2	| Да	| 469	| Нет |
| 70	| 2	| Да	| 329	| Нет |
| 80	| 2	| Да	| 239	| Нет |
| 90	| 2	| Да	| 105 |	Нет |



Таблица 3. Результаты экспериментов на шумоустойчивость

Амплитуда	Количество трикластеров Tri-k-means	Средняя дисперсия Tri-k-means	Количество трикластеров
NOAC	Средняя дисперсия NOAC
0	2	0	2	0
0.1	2	0,0031	2	0,0031
0.5	2	0,0775	2	0,0775
0.9	2	0,2767	562	0,2622
1	2	0,3424	967	0,3221
1.5	2	1,5556	1115	0,7573
2	2	2,9508	77	1,2769

Таблица 4. Результаты экспериментов с NOAC на реальных данных

Количество триплетов	Количество трикластеров 	Средняя дисперсия	Средняя плотность
10 000	13	0,4831	0,5269
20 000	47	0,6840	0,5208
30 000	101	0,7947	0,5399
40 000	153	0,7667	0,5436
50 000	259	0,8186	0,5509
60 000	372	0,8240	0,5471
70 000	618	0,8200	0,5434
80 000	864	0,8265	0,5471
90 000	1135	0,8545	0,5508
100 000	1421	0,8672	0,5527


Таблица 5. Результаты экспериментов с Tri-k-means на реальных данных

Количество триплетов	Количество трикластеров	Средняя дисперсия	Средняя плотность
10 000	13	1,0447	0,0126
20 000	47	0,8264	0,0717
30 000	101	0,8152	0,1057
40 000	153	0,7006	0,1253
50 000	259	0,6286	0,2041
60 000	372	0,5657	0,2300
70 000	618	0,4854	0,3190
80 000	864	0,4339	0,3777
90 000	1135	0,4303	0,3929
100 000	1421	0,4087	0,4422


Таблица 6. Сходимость метода Tri-k-means

| Количество триплетов, тыс. |	10 |	20 |	30 |	40 |	50 |	60 | 70 |	80 |	90 |	100 |
| -------------------------- |---- | --- | --- | --- | --- | --- | -- | -- | --- | ---- |
| Количество шагов	         | 3   |	2  |	3  |	3  |	2  |  3	 | 3	| 3	 |  3	 |  3   |


![Alt text](img/performance.jpg?raw=true "Зависимость времени работы от объёма выборки")

Диаграмма 1. Зависимость времени работы от объёма выборки
