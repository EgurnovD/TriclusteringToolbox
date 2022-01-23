// MovieLensPrepare.cpp: определяет точку входа для консольного приложения.
//
//#include "stdafx.h"
#include "iostream"
#include "fstream"
#include "string"
#include "vector"

using namespace std;

vector<string> MySplit(string s, string delim) 
{
	int pos;
	vector<string> tmp;
	while ((pos = s.find(delim)) != string::npos)
	{
		tmp.push_back(s.substr(0, pos));
		s.erase(0, pos + delim.length());
	}
	if (s != "") {
		tmp.push_back(s);
	}

	return tmp;
}

int main() //_tmain(int argc, _TCHAR* argv[])
{
	ifstream ITEM, DATA;
	ofstream CTX;
	ITEM.open("u.item");

	string s;
	vector<vector<string> > items;

	while (getline(ITEM,s))
		items.push_back(MySplit(s,"|"));

	ITEM.close();

	DATA.open("u.data");
	CTX.open("movie_lense.tctx");

	int user, item, rating;
	long time;

	while (DATA >> user >> item >> rating >> time)
	{
		for (int genre = 0; genre < 19; ++genre)
			if (items[item - 1][5 + genre] == "1") {
				CTX << user << '\t' << item << '\t' << genre << '\t' << rating << endl;
			}
	}

	DATA.close();
	CTX.close();

	return 0;
}

