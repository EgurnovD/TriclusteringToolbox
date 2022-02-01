# TriclusteringToolbox

OAC-Triclustering instruments

* This is a companion page for the paper on Triclustering of Real-Valued Triadic Contexts.

* Данная страница содержит дополнительные материалы к статье "ТРИКЛАСТЕРЫ БЛИЗКИХ ЗНАЧЕНИЙ ДЛЯ АНАЛИЗА ДАННЫХ ТРЁХМЕРНЫХ ТЕНЗОРОВ".


## 1	Программные средства
Для проведения экспериментов в рамках данного исследования было разработано несколько вспомогательных утилит. Из них стоит особенно выделить генератор вещественных триадических контекстов, позволяющий создавать контексты с заранее заданными свойствами. Все эксперименты проводились с использованием программного комплекса Triclustering Toolbox, первично разработанного и описанным в работе [1], но существенно расширенного и усовершенствованного в целях соответствия  требованиям проводимых экспериментов.

### 1.1	Генератор контекстов
Данное консольное приложение было написано на языке программирования C# в среде разработки MS Visual Studio 2012. Оно позволяет быстро создавать триадические контексты с определёнными параметрами для тестирования работоспособности алгоритмов без последующей интерпретации результатов.

Вначале программе требуется задать имя выходного файла, в который будет записан сгенерированный контекст. Затем указываются параметры кубоидов, из которых он будет состоять. Далее можно указать долю троек, пропускаемых при записи, таким образом имитируя неполноту информации или потерю данных. Также есть возможность произвести зашумление данных. Оно реализуется за счёт отклонения значений контекста на величину из диапазона [-A,A] (необходимо задать параметр А), распределённую случайно и равномерно в этом диапазоне.

Формат выходного файла совпадает с форматом файла, принимаемым на вход программным комплексом Triclustering Toolbox, и состоит из перечисления троек и соответствующих им вещественных значений. Описания располагаются по одному на строку, а значения внутри строки разделяются символами табуляции ("\t").

### 1.2	Инструментарий трикластеризации
Основной программой для проведения экспериментов в данной работе являлась Triclustering Toolbox. В ней содержатся реализации всех предложенных алгоритмов, интегрированы средства контроля времени их исполнения и вычисления оценок результатов кластеризации.

Изначально она была написана на языке C# на платформе .Net Framework 4 в среде Microsoft Visual Studio 2010 [1]. Доработка велась на том же языке, но в среде Microsoft Visual Studio 2012.

![Alt text](img/GUI.jpg?raw=true "Иллюстрация интерфейса программы Triclustering Toolbox")

Рисунок 1 Иллюстрация интерфейса программы Triclustering Toolbox

На рисунке 1 представлена иллюстрация интерфейса программы. 

На вход программа принимает контекст, записанный в файле с расширением ".tctx", в котором каждая строчка описывает одну тройку контекста. Значения в строках разделены символами табуляции. Путь к входному файлу необходимо указать в окне программы. Также интерфейс позволяет задать:
*	Выходную папку. В неё будут помещены результаты работы программы и журналы времени исполнения.
*	Потребность загрузить неполный контекст. Для пробной обработки части большого контекста можно задать количество первых троек, которые будут обработаны.
*	Для метода NOAC можно включить удаление трикластеров, при вложении по одному из измерений (объектов, признаков или условий), если плотность меньшего трикластера меньше плотности большого, или, в противном случае, если разница плотности достаточно мала (необходимо указать максимальное значение разности плотности).
*	Метод трикластеризации: NOAC или Tri-k-means.
*	Дополнительные опции алгоритма.

Как только все необходимые настройки будут установлены, можно запустить выбранный алгоритм, нажатием на кнопку "Start".

Опишем подробнее дополнительные опции, которые можно указать при вызове алгоритма. В метод NOAC нужно передать значение параметра δ, если оно отличается от значения "0", передаваемого по-умолчанию. От методов ОАС-трикластеризации была унаследована возможность не включать в выдачу трикластеры, плотность которых не достигает минимально допустимого значения. Также можно задать минимальную поддержку трикластеров по измерениям, т. е. минимальную мощность объёма, содержания и модуса. Метод Tri-k-means требует передачи количества кластеров, на которые нужно разбить контекст и значение параметра γ. Дополнительно можно передать  ограничение сверху на количество шагов алгоритма k-средних и способ задания начальных центроид для кластеров, который кодируется "0", если используется случайный выбор (передаётся по-умолчанию), или "1", если нужно выбрать заранее запрограммированные значения. Передаваемые опции разделяются запятыми, десятичные дроби пишутся через точку. В случае необходимости провести несколько экспериментов, наборы опций разделяются точкой с запятой.

В результате работы программы в выходной папке будут записаны два файла. В первом будут перечислены все обнаруженные трикластеры вместе с оценками. В первой строке этого файла пишется количество описанных трикластеров. Затем следует заголовочная строка, поясняющая  порядок описания трикластеров: Density, Variance, Average Coverage, Objects coverage, Attributes coverage, Conditions coverage, Extent, Intent, Modus (см. определения в исходном тексте и [2]). Далее перечисляются трикластеры в приведённом выше формате. Второй файл содержит журнал выполнения программы. Новая информация дописывается в конец файла. Каждая строчка соответствует  одному запуску метода и сообщает список переданных опций, время выполнения, число найденных трикластеров, общее покрытие трикластерами триплетов контекста и покрытие множеств объектов, признаков и условий по отдельности, а также значение меры разнообразия (diversity) как по всему контексту, так и по отдельным измерениям.

## 2	Данные
### 2.1	Синтетические данные
Утилитой ContextGenerator были созданы следующие контексты:
*	Эталонный контекст, состоящий из двух кубоидов размером 10х10х10 и 5х6х4 со значениями, соответсвенно, 3 и 7. Всего 1120 троек.
*	Контексты для экспериментов на пропущенные значения с уровнями потерь от 10% до 90% с шагом 10%.
*	Размытые контексты с амплитудой размытия 0.1, 0.5, 0.9, 1, 1.5 и 2.



### 2.2. Реальные данные
Для проверки качества трикластеризации с возможностью последующей интерпретации результатов использовался набор данных 100k проекта GroupLens, содержащий информацию о 100 000 оцениваниях на сайте MovieLens по пятибальной шкале, проведенных 1000 пользователями для 1700 фильмов при наличии 19 невзаимоисключающих жанров. В наборе также указано, к каким жанрам относятся фильмы. В качестве множества объектов были взяты пользователи, множества признаков – фильмы, множества условий – жанры. Тройки составлялись из идентификатора пользователя, фильма, оценённого пользователем и одного из жанров, приписанных фильму. Так как жанр является дополнительной модальностью данных, то все тройки, соответствующие паре пользователь-фильм означивались одинаково при наличии более одного жанра у фильма. 

Таблица 1. Контекст GroupLens

|           |	#G     | #M   |	#B    |	Количество троек   |	Плотность контекста   |
| --------- | ----- | ----- | ----- | ------------------ | ---------------------- |
| GroupLens	| 1000  | 1700  |	19    |	  212595           |       	0,00658         |


## 3 Результаты экспериментов
Все измерения производились на компьютере с процессором Intel Core i5-M430 2.27 ГГц и 3 ГБ оперативной памяти под управлением 64-разрядной версии Microsoft Windows 7 SP1.

В ходе предварительного тестирования было выявлено, что для сравнения результатов работы предложенных методов сообразно брать параметр γ метода Tri-k-means равным параметру δ метода NOAC, используемому в аналогичном случае.

###  3.1 Устойчивость к пропущенным значениям
В таблице 2 находятся результаты испытаний при отсутствии части данных.  Они демонстрируют неплохую устойчивость метода Tri-k-means к неполноте информации. Метод NOAC напротив оказался довольно требовательным к плотности входных контекстов. Уже на уровне потерь 20% исходные кубоиды не были обнаружены методом, хотя многие триклатеры, количество которых было соизмеримо с количеством оставшихся в контексте триплетов, были близки к ним.  

В этих экспериментах параметры Tri-k-means k = 2, γ = 0; параметр NOAC δ = 0.

Таблица 2. Результаты экспериментов при отсутствие части данных

| % потерь |	Количество трикластеров Tri-k-means |	Обнаружение исходных кубоидов Tri-k-means |	Количество трикластеров NOAC	| Обнаружение исходных кубоидов NOAC |
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

### 3.2 Шумоустойчивость
Шумоустойчивость методов проверялась на эталонном контексте с различными амплитудами отклонений в обе стороны. В этих экспериментах использовались Tri-k-means с параметрами k = 2, γ = 1; NOAC параметр δ = 1.

Таблица 3 содержит результаты экспериментов на шумоустойчивость. По приведённым оценкам видно, что при малых отклонениях значений оба метода хорошо справляются с поиском трикластеров близких значений. С ростом амплитуды размытия Tri-k-means начинает быстро терять в оценке дисперсии обнаруживаемых трикластеров. Начиная с отметки 1.5, он выдаёт трикластеры, не удовлетворяющие условию близости значений. Метод NOAC сохраняет близость значений трикластеров с увеличением размытия, но помимо искомых, обнаруживает множество схожих трикластеров, в количестве соизмеримом с общим количеством троек. Возможным решением этой проблемы является фильтрация и объединение похожих трикластеров. Оба метода перестают удовлетворять условию близости значений трикластеров при амплитуде равной 2, т.е. когда диапазоны размытых значений двух эталонных трикластеров пересекаются.

Таблица 3. Результаты экспериментов на шумоустойчивость

|Амплитуда | Количество трикластеров Tri-k-means | Средняя дисперсия Tri-k-means | Количество трикластеров NOAC | Средняя дисперсия NOAC |
| -- | -- | -- | -- | -- |
| 0 | 2 | 0 | 2 |0 |
| 0,1 | 2 | 0,0031 | 2 | 0,0031 |
| 0,5 | 2 | 0,0775 | 2 | 0,0775 |
| 0,9 | 2 | 0,2767 | 562 | 0,2622 |
| 1 | 2 | 0,3424 | 967 | 0,3221 |
| 1,5 | 2 | 1,5556 | 1115 | 0,7573 |
| 2 | 2 | 2,9508 | 77 | 1,2769 |

## 3.3 Реальные данные
В силу длительного времени исполнения программ на больших объёмах данных, было решено не обрабатывать более 100000 триплетов за раз.

В таблицах 4 и 5 содержится информация об экспериментах над реальными данными. Количество трикластеров в методе Tri-k-means задавалось вручную и бралось равным количеству трикластеров, обнаруженному NOAC в соответствующем эксперименте. Параметры γ и δ равнялись 1. Метод NOAC использовался с отсечением трикластеров, плотность которых меньше 0,5 или поддержка по объёму и признакам меньше 4, с целью сократить время работы и повысить качество выходных трикластеров.

Таблица 4. Результаты экспериментов с NOAC на реальных данных

| Количество триплетов | Количество трикластеров  | Средняя дисперсия | Средняя плотность |
| -- | -- | -- | -- |
| 10 000 | 13 | 0,4831 | 0,5269 |
| 20 000 | 47 | 0,6840 | 0,5208 |
| 30 000 | 101 | 0,7947 | 0,5399 |
| 40 000 | 153 | 0,7667 | 0,5436 |
| 50 000 | 259 | 0,8186 | 0,5509 |
| 60 000 | 372 | 0,8240 | 0,5471 |
| 70 000 | 618 | 0,8200 | 0,5434 |
| 80 000 | 864 | 0,8265 | 0,5471 |
| 90 000 | 1135 | 0,8545 | 0,5508 |
| 100 000 | 1421 | 0,8672 | 0,5527 |

Таблица 5. Результаты экспериментов с Tri-k-means на реальных данных

| Количество триплетов | Количество трикластеров | Средняя дисперсия | Средняя плотность |
| -- | -- | -- | -- |
| 10 000 | 13 | 1,0447 | 0,0126 |
| 20 000 | 47 | 0,8264 | 0,0717 |
| 30 000 | 101 | 0,8152 | 0,1057 |
| 40 000 | 153 | 0,7006 | 0,1253 |
| 50 000 | 259 | 0,6286 | 0,2041 |
| 60 000 | 372 | 0,5657 | 0,2300 |
| 70 000 | 618 | 0,4854 | 0,3190 |
| 80 000 | 864 | 0,4339 | 0,3777 |
| 90 000 | 1135 | 0,4303 | 0,3929 |
| 100 000 | 1421 | 0,4087 | 0,4422 |

Из таблицы 6 следует, что наше предположение о скорости сходимости метода Tri-k-means было оправдано.

Таблица 6. Сходимость метода Tri-k-means

| Количество триплетов, тыс. |	10 |	20 |	30 |	40 |	50 |	60 | 70 |	80 |	90 |	100 |
| -------------------------- | --- | --- | --- | --- | --- | --- | -- | -- | --- | ---- |
| Количество шагов	         | 3   |	2  |	3  |	3  |	2  |  3	 | 3	| 3	 |  3	 |  3   |


![Alt text](img/performance.jpg?raw=true "Зависимость времени работы от объёма выборки")

Рис. 2. Зависимость времени работы от объёма выборки

Рис. 2 демонстрирует, что время работы метода Tri-k-means имеет меньший порядок роста при увеличении объёма входных данных, чем NOAC.
Полученные в результате обработки данного реального контекста трикластеры можно интерпретировать, как клубы по интересам, состоящие из пользователей одинаково оценивших схожие по жанрам фильмы. Отсутствующие значения в таком случае можно применить в рекомендательной системе, предполагая, что новые оценки будут дополнять существующие трикластеры, не сильно отклоняясь от среднего значения.

## Список литературы

1. Гнатышак Д. В. Сравнительный анализ методов трикластеризации и их приложения. – Москва, 2012
2. Dmitry I. Ignatov, Dmitry V. Gnatyshak, Sergei O. Kuznetsov, Boris G. Mirkin:
Triadic Formal Concept Analysis and triclustering: searching for optimal patterns. Mach. Learn. 101(1-3): 271-302 (2015)

