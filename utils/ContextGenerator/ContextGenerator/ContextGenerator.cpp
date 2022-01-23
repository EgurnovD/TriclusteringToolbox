#include "stdafx.h"
#include <iostream>
#include <stdio.h>
#include <string>
#include <vector>

using namespace std;

class block {
public:
	int x, y, z;
	int l, w, h;
	double val;
	double ampl;
	double lostp;

	block(int nx = 0, int ny = 0, int nz = 0, int nl = 0, int nw = 0, int nh = 0, double nval = 0, double nampl = 0, double nlostp = 0) {
		x = nx;
		y = ny;
		z = nz;
		l = nl;
		w = nw;
		h = nh;
		val = nval;
		ampl = nampl;
		lostp = nlostp;
	}

	bool inside(int a, int b, int c) {
		 return (x <= a) && (a < x + l) && (y <= b) && (b < y + w) && (z <= c) && (c < z + h);
	 }
};

int _tmain(int argc, _TCHAR* argv[])
{
	string filename = "context";
	//cout << "Enter context filaname (without extension): ";
	//cin >> filename;

	//const int l = 20, w = 20, h = 20;
	//double data[l][w][h];

	vector<block> blocks;
	//b.push_back(block(1,1,1,2,3,4,1));

	//b.push_back(block(1,1,1,10,10,10,10));
	//b.push_back(block(2,6,12,7,7,7,7));
	//b.push_back(block(11,8,3,5,5,5,5));

	//b.push_back(block(1,2,1,2,2,1,3));
	//b.push_back(block(3,1,1,2,2,1,7));

	//blocks.push_back(block(1,1,1,2,2,1,3));
	//blocks.push_back(block(3,1,1,1,3,1,7));
	//blocks.push_back(block(1,3,1,2,1,1,7));

	blocks.push_back(block(1,1,1,10,10,10,3));
	blocks.push_back(block(11,8,7,5,6,4,7));

	double a[] = {0.1, 0.9, 1, 1.5};

	//for (int tmp = 0; tmp < 4; tmp++)
	for (int tmp = 0; tmp < 10; tmp+=10)
	{
		double amplitude = 0.5;
		double lost_percent = (double)tmp;

		string f = filename + "_l_" + std::to_string((int)lost_percent) + "_amp_" + std::to_string(amplitude) + ".tctx";
		freopen(f.c_str(), "w", stdout);

		////suppose the blocks doesn't intersect
		//for (int i = 0; i < l; ++i) {
		//	for (int j = 0; j < w; ++j) {
		//		for (int k = 0; k < h; ++k) {
		//			data[i][j][k] = 0;
		//			for (int u = 0; u < blocks.size(); ++u) {
		//				if (blocks[u].inside(i,j,k) && !((rand() % 100 ) < lost_percent))
		//				{
		//					double noise = ((double)rand() / RAND_MAX ) * (2 * amplitude) - amplitude;
		//						//rand() % (amplitude * 2 + 1) - amplitude; // data +/- amplitude
		//					data[i][j][k] = blocks[u].val + noise;
		//				}
		//			}
		//		}
		//	}
		//}

		//// output in the format
		//for (int i = 0; i < l; ++i) {
		//	for (int j = 0; j < w; ++j) {
		//		for (int k = 0; k < h; ++k) {
		//			if (data[i][j][k] != 0) {
		//				printf("a%d\tb%d\tc%d\t%.0lf\n", i, j, k, data[i][j][k]);
		//				// printf("a%d\tb%d\tc%d\n", i, j, k);
		//			}
		//		}
		//	}
		//}

		for (int u = 0; u < blocks.size(); ++u)
		{
			block b = blocks[u];
			for (int i = b.x; i < b.x + b.l; ++i)
				for (int j = b.y; j < b.y + b.w; ++j)
					for (int k = b.z; k < b.z + b.h; ++k)
						if (((double)rand() / RAND_MAX ) * 100 >= lost_percent)
						{
							double noise = ((double)rand() / RAND_MAX ) * (2 * amplitude) - amplitude; // data +/- amplitude
							double data = b.val + noise;
							printf("a%d\tb%d\tc%d\t%.4lf\n", i, j, k, data);
						}
		}

		fclose(stdout);
	}

	return 0;
}

